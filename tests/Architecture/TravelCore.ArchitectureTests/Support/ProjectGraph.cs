using System.Xml.Linq;

namespace TravelCore.ArchitectureTests.Support;

internal sealed record ProjectModel(
    string Path,
    string RelativePath,
    string Name,
    IReadOnlyList<string> ProjectReferences,
    IReadOnlyList<string> PackageReferences)
{
    public bool IsUnderSrc => RelativePath.Replace('\\', '/').StartsWith("src/", StringComparison.OrdinalIgnoreCase);

    public bool IsUnderTests => RelativePath.Replace('\\', '/').StartsWith("tests/", StringComparison.OrdinalIgnoreCase);

    public bool IsModuleDomain =>
        RelativePath.Replace('\\', '/').Contains("/Modules/", StringComparison.OrdinalIgnoreCase)
        && Name.EndsWith(".Domain", StringComparison.OrdinalIgnoreCase);
}

internal static class ProjectGraph
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TravelCore.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate TravelCore.sln from test base directory.");
    }

    public static IReadOnlyList<ProjectModel> LoadAll(string repoRoot)
    {
        return Directory.EnumerateFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(p => Parse(repoRoot, p))
            .OrderBy(p => p.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static ProjectModel Parse(string repoRoot, string projectPath)
    {
        var doc = XDocument.Load(projectPath);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
        var projectRefs = doc.Descendants(ns + "ProjectReference")
            .Select(x => (string?)x.Attribute("Include") ?? string.Empty)
            .Where(x => x.Length > 0)
            .Select(x => x.Replace('/', Path.DirectorySeparatorChar))
            .ToList();
        var packageRefs = doc.Descendants(ns + "PackageReference")
            .Select(x => (string?)x.Attribute("Include") ?? string.Empty)
            .Where(x => x.Length > 0)
            .ToList();

        return new ProjectModel(
            projectPath,
            Path.GetRelativePath(repoRoot, projectPath),
            Path.GetFileNameWithoutExtension(projectPath),
            projectRefs,
            packageRefs);
    }

    public static IReadOnlyList<string> FindProductionToTestViolations(IEnumerable<ProjectModel> projects)
    {
        var list = new List<string>();
        foreach (var project in projects.Where(p => p.IsUnderSrc))
        {
            foreach (var reference in project.ProjectReferences)
            {
                var normalized = reference.Replace('\\', '/');
                if (normalized.Contains("/tests/", StringComparison.OrdinalIgnoreCase)
                    || normalized.StartsWith("tests/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("../tests/", StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains("..\\tests\\", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add($"{project.RelativePath} -> {reference}");
                }

                // Resolve relative to project directory when possible.
                var absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project.Path)!, reference));
                var absoluteNormalized = absolute.Replace('\\', '/');
                if (absoluteNormalized.Contains("/tests/", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add($"{project.RelativePath} -> {reference}");
                }
            }
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static IReadOnlyList<string> FindForbiddenDomainDependencies(IEnumerable<ProjectModel> projects)
    {
        var forbiddenPackages = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.EntityFrameworkCore.Design",
            "Npgsql",
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime"
        };

        var list = new List<string>();
        foreach (var project in projects.Where(p => p.IsModuleDomain))
        {
            foreach (var package in project.PackageReferences)
            {
                if (forbiddenPackages.Any(f => package.Equals(f, StringComparison.OrdinalIgnoreCase)
                    || package.StartsWith(f + ".", StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add($"{project.RelativePath} PackageReference {package}");
                }
            }

            foreach (var reference in project.ProjectReferences)
            {
                var name = Path.GetFileNameWithoutExtension(reference);
                if (name.Equals("TravelCore.Api", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("TravelCore.ApiFoundation", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("TravelCore.Persistence.PostgreSql", StringComparison.OrdinalIgnoreCase)
                    || reference.Replace('\\', '/').Contains("/tests/", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add($"{project.RelativePath} ProjectReference {reference}");
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Synthetic evaluation for unit-testing the Domain dependency rule without mutating the repo.
    /// </summary>
    public static IReadOnlyList<string> EvaluateDomainRules(ProjectModel syntheticDomainProject)
        => FindForbiddenDomainDependencies([syntheticDomainProject]);
}
