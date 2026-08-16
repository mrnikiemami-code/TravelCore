using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Destination.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P07-T003 / P07-R2: Place DestinationId is contract-validated logical association only.
/// </summary>
public sealed class PlaceDestinationLinkGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void IDestinationExistenceQuery_IsContractsSurface_Only()
    {
        Assert.Equal(
            "TravelCore.Modules.Destination.Contracts",
            typeof(IDestinationExistenceQuery).Namespace);
        Assert.NotNull(typeof(IDestinationExistenceQuery).GetMethod(nameof(IDestinationExistenceQuery.ExistsAsync)));
    }

    [Fact]
    public void PlaceInfrastructure_MustNotProjectReference_DestinationInfrastructureOrDomain()
    {
        var placeInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Place.Infrastructure");
        var violations = placeInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Equals("TravelCore.Modules.Destination.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Destination.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Place.Infrastructure must depend on Destination.Contracts only:\n"
            + string.Join('\n', violations));

        Assert.Contains(
            placeInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name.Equals("TravelCore.Modules.Destination.Contracts", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PlaceDomain_HasNoDestinationEntityNavigationOrCrossSchemaFkHints()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        Assert.True(Directory.Exists(placeRoot), placeRoot);

        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(DestinationDbContext|HasOne<\s*Destination|WithMany\(.*Destination|principalSchema:\s*""destination""|Modules\.Destination\.Domain)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Place must not introduce Destination EF navigation or cross-schema FK:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PlaceSchema_ForbidsNameFaNameEnColumnsInSource()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(x.line, @"\b(NameFa|NameEn|name_fa|name_en)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Place must not introduce NameFa/NameEn columns:\n" + string.Join('\n', hits));
    }
}
