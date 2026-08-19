using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionProviderExpansionBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void EvolutionProviderExpansionBoundary_Is_Declared()
    {
        Assert.True(EvolutionProviderExpansionBoundary.ProviderExpansionBoundaryImplemented);
        Assert.False(EvolutionProviderExpansionBoundary.ProviderRegistryProductImplemented);
    }

    [Fact]
    public void PostP29_Evidence_Records_T005_And_Provider_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T005", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionProviderExpansionBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R3", plan, StringComparison.Ordinal);
    }
}
