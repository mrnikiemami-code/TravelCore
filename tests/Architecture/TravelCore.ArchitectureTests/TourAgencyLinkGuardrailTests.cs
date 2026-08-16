using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Party.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T005 / P09-R3: Tour→Agency (PartyKind.Agency) is contract-validated logical association only.
/// </summary>
public sealed class TourAgencyLinkGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void IPartyReadQuery_IsContractsSurface_Only()
    {
        Assert.Equal(
            "TravelCore.Modules.Party.Contracts",
            typeof(IPartyReadQuery).Namespace);
        Assert.NotNull(typeof(IPartyReadQuery).GetMethod(nameof(IPartyReadQuery.GetAsync)));
    }

    [Fact]
    public void TourInfrastructure_MustReference_PartyContracts_Only()
    {
        var tourInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Tour.Infrastructure");
        var refs = tourInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .ToList();

        Assert.Contains(
            refs,
            name => name.Equals("TravelCore.Modules.Party.Contracts", StringComparison.OrdinalIgnoreCase));

        var violations = refs
            .Where(name =>
                name.Equals("TravelCore.Modules.Party.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Party.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Tour.Infrastructure must depend on Party.Contracts only:\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void TourAgencyLinks_AreLogicalWithoutCrossSchemaFk()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"HasOne<.*Party|TravelCore\.Modules\.Party\.Domain|principalSchema:\s*""party""|PartyDbContext")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour must not introduce Party schema FK/navigation:\n" + string.Join('\n', hits));

        Assert.Contains(
            "AgencyId",
            File.ReadAllText(Path.Combine(tourRoot, "TravelCore.Modules.Tour.Domain", "TourProduct.cs")),
            StringComparison.Ordinal);
    }
}
