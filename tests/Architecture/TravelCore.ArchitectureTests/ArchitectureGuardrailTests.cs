using System.Reflection;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Identifiers;
using TravelCore.Persistence.PostgreSql;
using TravelCore.PersistenceFixture;
using TravelCore.Time;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using MoneyType = TravelCore.Money.Money;
using ReflectionAssembly = System.Reflection.Assembly;

namespace TravelCore.ArchitectureTests;

public sealed class ArchitectureGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Uuid7).Assembly,
            typeof(TravelCoreTemporal).Assembly,
            typeof(MoneyType).Assembly,
            typeof(PostgreSqlProviderExtensions).Assembly,
            typeof(PersistenceFixtureDbContext).Assembly,
            LoadApiAssembly())
        .Build();

    [Fact]
    public void ProductionProjects_MustNotReferenceTests()
    {
        var production = Projects.Where(p => p.IsUnderSrc).ToList();
        Assert.False(production.Count == 0, "Expected production projects under src/.");

        var violations = ProjectGraph.FindProductionToTestViolations(Projects);
        Assert.True(
            violations.Count == 0,
            "Production projects must not reference tests:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void Api_MustNotReferencePersistenceFixture()
    {
        var api = Projects.Single(p => p.Name == "TravelCore.Api");
        var hit = api.ProjectReferences.Any(r =>
            Path.GetFileNameWithoutExtension(r).Equals("TravelCore.PersistenceFixture", StringComparison.OrdinalIgnoreCase));
        Assert.False(hit, "TravelCore.Api must not ProjectReference TravelCore.PersistenceFixture.");
    }

    [Fact]
    public void PurePrimitives_MustRemainFrameworkIndependent()
    {
        var primitives = Projects
            .Where(p => p.Name is "TravelCore.Identifiers" or "TravelCore.Time" or "TravelCore.Money")
            .ToList();
        Assert.Equal(3, primitives.Count);

        string[] forbiddenPackages =
        [
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        ];

        string[] forbiddenProjects =
        [
            "TravelCore.Persistence.PostgreSql",
            "TravelCore.Api",
            "TravelCore.ApiFoundation",
            "TravelCore.PersistenceFixture"
        ];

        var violations = new List<string>();
        foreach (var project in primitives)
        {
            foreach (var package in project.PackageReferences)
            {
                if (forbiddenPackages.Any(f => package.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"{project.Name} PackageReference {package}");
                }
            }

            foreach (var reference in project.ProjectReferences)
            {
                var name = Path.GetFileNameWithoutExtension(reference);
                if (forbiddenProjects.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    violations.Add($"{project.Name} ProjectReference {name}");
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join('\n', violations));
    }

    [Fact]
    public void PersistenceFixture_MustRemainOutsideProduction()
    {
        var fixture = Projects.Single(p => p.Name == "TravelCore.PersistenceFixture");
        var relative = fixture.RelativePath.Replace('\\', '/');
        Assert.StartsWith("tests/", relative, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("src/backend/Modules/", relative, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("src/backend/Platform/", relative, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedPostgreSqlProvider_MustNotOwnDbContext()
    {
        var types = Architecture.Types
            .Where(t => t.FullName?.StartsWith("TravelCore.Persistence.PostgreSql", StringComparison.Ordinal) == true)
            .ToList();
        Assert.False(types.Count == 0, "Expected TravelCore.Persistence.PostgreSql types to be loaded.");

        Classes()
            .That().ResideInNamespace("TravelCore.Persistence.PostgreSql")
            .Should().NotBeAssignableTo(typeof(DbContext))
            .Check(Architecture);
    }

    [Fact]
    public void ApiHost_MustNotOwnDbContext()
    {
        var api = LoadApiAssembly();
        var offenders = api.GetTypes()
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && t != typeof(DbContext))
            .Select(t => t.FullName!)
            .ToList();
        Assert.True(offenders.Count == 0, "TravelCore.Api must not own DbContext:\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void PlatformProjects_MustNotOwnDbContext()
    {
        var platformProjects = Projects
            .Where(p =>
            {
                var relative = p.RelativePath.Replace('\\', '/');
                return relative.Contains("src/backend/Platform/", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
        Assert.False(platformProjects.Count == 0, $"Expected Platform projects. Discovered={Projects.Count}");

        var offenders = new List<string>();
        foreach (var project in platformProjects)
        {
            var dll = Path.Combine(
                Path.GetDirectoryName(project.Path)!,
                "bin",
                "Debug",
                "net10.0",
                project.Name + ".dll");
            if (!File.Exists(dll))
            {
                // Fallback: already-loaded assemblies from project references.
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == project.Name);
                if (loaded is null)
                {
                    continue;
                }

                offenders.AddRange(FindDbContexts(loaded, project.Name));
                continue;
            }

            offenders.AddRange(FindDbContexts(ReflectionAssembly.LoadFrom(dll), project.Name));
        }

        Assert.True(offenders.Count == 0, "Platform must not own DbContext types:\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void GlobalDbContextNames_AreForbidden()
    {
        string[] forbidden =
        [
            "TravelCoreDbContext",
            "ApplicationDbContext",
            "GlobalDbContext",
            "SharedDbContext"
        ];

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("TravelCore", StringComparison.Ordinal) == true);

        var offenders = assemblies
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
            })
            .Where(t => t is not null && forbidden.Contains(t!.Name, StringComparer.Ordinal))
            .Where(t => typeof(DbContext).IsAssignableFrom(t!))
            .Select(t => t!.FullName!)
            .ToList();

        Assert.True(offenders.Count == 0, "Forbidden global DbContext names:\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void DumpingGroundProjectNames_AreForbidden()
    {
        string[] forbiddenNames =
        [
            "TravelCore.SharedKernel",
            "TravelCore.Common",
            "TravelCore.Utilities"
        ];

        var offenders = Projects
            .Where(p => p.IsUnderSrc)
            .Where(p => forbiddenNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .Select(p => p.RelativePath)
            .ToList();

        Assert.True(offenders.Count == 0, "Forbidden dumping-ground projects:\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void FixturePersistence_MustUseOwnedSchemaAndOutbox()
    {
        var options = new DbContextOptionsBuilder<PersistenceFixtureDbContext>()
            .UseTravelCorePostgreSql(
                "Host=127.0.0.1;Database=architecture_guard_design;Username=architecture;Password=not-a-real-secret",
                migrationsHistorySchema: PersistenceFixtureDbContext.SchemaName)
            .Options;

        using var db = new PersistenceFixtureDbContext(options);
        Assert.Equal(System.Data.ConnectionState.Closed, db.Database.GetDbConnection().State);
        Assert.Equal("p01_fixture", PersistenceFixtureDbContext.SchemaName);

        var entityNames = db.Model.GetEntityTypes().Select(e => e.ClrType.Name).OrderBy(x => x).ToList();
        Assert.Contains("PersistenceProbe", entityNames);
        Assert.Contains("PersistenceFixtureOutboxMessage", entityNames);
        Assert.Equal(2, entityNames.Count);

        foreach (var name in entityNames)
        {
            var entity = db.Model.GetEntityTypes().Single(e => e.ClrType.Name == name);
            Assert.Equal("p01_fixture", entity.GetSchema());
        }

        Assert.Equal(System.Data.ConnectionState.Closed, db.Database.GetDbConnection().State);
    }

    [Fact]
    public void SeoPersistence_MustUseOwnedSchema_seo()
    {
        var options = new DbContextOptionsBuilder<TravelCore.Modules.Seo.Infrastructure.SeoDbContext>()
            .UseTravelCorePostgreSql(
                "Host=127.0.0.1;Database=architecture_guard_seo_design;Username=architecture;Password=not-a-real-secret",
                migrationsHistorySchema: TravelCore.Modules.Seo.Infrastructure.SeoDbContext.SchemaName)
            .Options;

        using var db = new TravelCore.Modules.Seo.Infrastructure.SeoDbContext(options);
        Assert.Equal(System.Data.ConnectionState.Closed, db.Database.GetDbConnection().State);
        Assert.Equal("seo", TravelCore.Modules.Seo.Infrastructure.SeoDbContext.SchemaName);

        // Product entities (e.g. SeoRoute) must remain under schema seo.
        Assert.Equal("seo", db.Model.GetDefaultSchema());
        foreach (var entity in db.Model.GetEntityTypes())
        {
            Assert.Equal("seo", entity.GetSchema());
        }

        Assert.Equal(System.Data.ConnectionState.Closed, db.Database.GetDbConnection().State);
    }

    [Fact]
    public void FixtureMigrations_MustRemainUnderFixtureProject()
    {
        var migrationsDir = Path.Combine(
            RepoRoot,
            "tests",
            "Fixtures",
            "Persistence",
            "TravelCore.PersistenceFixture",
            "Migrations");
        Assert.True(Directory.Exists(migrationsDir), $"Missing migrations folder: {migrationsDir}");

        var files = Directory.GetFiles(migrationsDir, "*.cs");
        Assert.False(files.Length == 0, "Expected fixture migration artifacts.");
        Assert.Contains(files, f => Path.GetFileName(f).Contains("PersistenceFixtureDbContextModelSnapshot", StringComparison.Ordinal));

        var forbiddenRoots = new[]
        {
            Path.Combine(RepoRoot, "src", "backend", "Platform", "Persistence", "PostgreSql"),
            Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api")
        };

        foreach (var root in forbiddenRoots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            var leaked = Directory.EnumerateFiles(root, "*ModelSnapshot.cs", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(root, "*Migration*.cs", SearchOption.AllDirectories))
                .Where(f => Path.GetFileName(f).Contains("PersistenceFixture", StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.True(leaked.Count == 0, "Fixture migrations leaked:\n" + string.Join('\n', leaked));
        }
    }

    [Fact]
    public void ArchitectureTestPackages_MustNotLeakIntoProduction()
    {
        string[] forbidden =
        [
            "xunit",
            "xunit.v3",
            "TngTech.ArchUnitNET",
            "TngTech.ArchUnitNET.xUnitV3",
            "Microsoft.NET.Test.Sdk"
        ];

        var violations = Projects
            .Where(p => p.IsUnderSrc)
            .SelectMany(p => p.PackageReferences
                .Where(pkg => forbidden.Any(f => pkg.Equals(f, StringComparison.OrdinalIgnoreCase)
                    || pkg.StartsWith(f + ".", StringComparison.OrdinalIgnoreCase)))
                .Select(pkg => $"{p.RelativePath}: {pkg}"))
            .ToList();

        Assert.True(violations.Count == 0, string.Join('\n', violations));
    }

    [Fact]
    public void FutureDomainProjects_HaveDependencyRuleEngine()
    {
        var violations = ProjectGraph.FindForbiddenDomainDependencies(Projects);
        Assert.True(violations.Count == 0, string.Join('\n', violations));
    }

    [Fact]
    public void Synthetic_ProductionToTestReference_IsDetected()
    {
        var synthetic = new ProjectModel(
            Path: Path.Combine(RepoRoot, "src", "backend", "Fake", "Fake.csproj"),
            RelativePath: "src/backend/Fake/Fake.csproj",
            Name: "Fake",
            ProjectReferences: ["../../../tests/SomeTestProject/SomeTestProject.csproj"],
            PackageReferences: []);

        var violations = ProjectGraph.FindProductionToTestViolations([synthetic]);
        Assert.False(violations.Count == 0, "Expected detector to flag production -> tests reference.");
    }

    [Fact]
    public void Synthetic_TestToProductionReference_IsAllowedByDetector()
    {
        var synthetic = new ProjectModel(
            Path: Path.Combine(RepoRoot, "tests", "Architecture", "TravelCore.ArchitectureTests", "TravelCore.ArchitectureTests.csproj"),
            RelativePath: "tests/Architecture/TravelCore.ArchitectureTests/TravelCore.ArchitectureTests.csproj",
            Name: "TravelCore.ArchitectureTests",
            ProjectReferences: ["../../../src/backend/Platform/Money/TravelCore.Money/TravelCore.Money.csproj"],
            PackageReferences: []);

        var violations = ProjectGraph.FindProductionToTestViolations([synthetic]);
        Assert.Empty(violations);
    }

    [Fact]
    public void Synthetic_DomainForbiddenDependencies_AreDetected()
    {
        var withEf = new ProjectModel(
            "x",
            "src/backend/Modules/Tour/TravelCore.Modules.Tour.Domain/TravelCore.Modules.Tour.Domain.csproj",
            "TravelCore.Modules.Tour.Domain",
            [],
            ["Microsoft.EntityFrameworkCore"]);
        Assert.Contains(
            ProjectGraph.EvaluateDomainRules(withEf),
            v => v.Contains("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));

        var withNpgsql = withEf with { PackageReferences = ["Npgsql.EntityFrameworkCore.PostgreSQL"] };
        Assert.Contains(
            ProjectGraph.EvaluateDomainRules(withNpgsql),
            v => v.Contains("Npgsql", StringComparison.OrdinalIgnoreCase));

        var withProvider = withEf with
        {
            PackageReferences = [],
            ProjectReferences = ["../../../../Platform/Persistence/PostgreSql/TravelCore.Persistence.PostgreSql/TravelCore.Persistence.PostgreSql.csproj"]
        };
        Assert.Contains(
            ProjectGraph.EvaluateDomainRules(withProvider),
            v => v.Contains("TravelCore.Persistence.PostgreSql", StringComparison.Ordinal));

        var clean = withEf with { PackageReferences = [], ProjectReferences = [] };
        Assert.Empty(ProjectGraph.EvaluateDomainRules(clean));
    }

    [Fact]
    public void DbContextOwnership_CurrentSubjectsAreNonVacuous()
    {
        var fixtureDbContexts = typeof(PersistenceFixtureDbContext).Assembly.GetTypes()
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && t != typeof(DbContext))
            .Select(t => t.Name)
            .ToList();
        Assert.Contains("PersistenceFixtureDbContext", fixtureDbContexts);

        var providerDbContexts = typeof(PostgreSqlProviderExtensions).Assembly.GetTypes()
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && t != typeof(DbContext))
            .ToList();
        Assert.Empty(providerDbContexts);
    }

    private static IEnumerable<string> FindDbContexts(ReflectionAssembly assembly, string projectName)
    {
        System.Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).Cast<System.Type>().ToArray();
        }

        return types
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && t != typeof(DbContext))
            .Select(t => $"{projectName}:{t.FullName}");
    }

    private static ReflectionAssembly LoadApiAssembly()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "TravelCore.Api");
        if (loaded is not null)
        {
            return loaded;
        }

        var dll = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "TravelCore.Api",
            "bin",
            "Debug",
            "net10.0",
            "TravelCore.Api.dll");
        return ReflectionAssembly.LoadFrom(dll);
    }
}
