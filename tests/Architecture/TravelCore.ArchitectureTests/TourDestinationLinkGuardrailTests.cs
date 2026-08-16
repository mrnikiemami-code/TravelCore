using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Destination.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T004 / P09-R2: Tour→Destination links are contract-validated logical associations only.
/// </summary>
public sealed class TourDestinationLinkGuardrailTests
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
    public void TourInfrastructure_MustReference_DestinationContracts_Only()
    {
        var tourInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Tour.Infrastructure");
        var refs = tourInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .ToList();

        Assert.Contains(
            refs,
            name => name.Equals("TravelCore.Modules.Destination.Contracts", StringComparison.OrdinalIgnoreCase));

        var violations = refs
            .Where(name =>
                name.Equals("TravelCore.Modules.Destination.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Destination.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Tour.Infrastructure must depend on Destination.Contracts only:\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void TourDestinationLinks_AreLogicalWithoutCrossSchemaFk()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"HasOne<.*Destination|TravelCore\.Modules\.Destination\.Domain|principalSchema:\s*""destination""|DestinationDbContext")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour must not introduce Destination schema FK/navigation:\n" + string.Join('\n', hits));

        var linkPath = Path.Combine(
            tourRoot,
            "TravelCore.Modules.Tour.Domain",
            "TourProductDestination.cs");
        Assert.True(File.Exists(linkPath), linkPath);
        Assert.Contains("public Guid DestinationId", File.ReadAllText(linkPath), StringComparison.Ordinal);
        Assert.Contains("OriginDestinationId", File.ReadAllText(
            Path.Combine(tourRoot, "TravelCore.Modules.Tour.Domain", "TourProduct.cs")), StringComparison.Ordinal);
    }
}
