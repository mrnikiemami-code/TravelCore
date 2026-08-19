using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionLoyaltyPromotionsBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void EvolutionLoyaltyPromotionsBoundary_Is_Declared()
    {
        Assert.True(EvolutionLoyaltyPromotionsBoundary.LoyaltyPromotionsBoundaryImplemented);
        Assert.False(EvolutionLoyaltyPromotionsBoundary.LoyaltyEngineImplemented);
    }

    [Fact]
    public void PostP29_Evidence_Records_T007_And_Loyalty_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T007", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionLoyaltyPromotionsBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R5", plan, StringComparison.Ordinal);
    }
}
