using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionPersonalizationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void EvolutionPersonalizationBoundary_Is_Declared()
    {
        Assert.True(EvolutionPersonalizationBoundary.PersonalizationBoundaryImplemented);
        Assert.False(EvolutionPersonalizationBoundary.RecommendationEngineImplemented);
    }

    [Fact]
    public void PostP29_Evidence_Records_T006_And_Personalization_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T006", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionPersonalizationBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R4", plan, StringComparison.Ordinal);
    }
}
