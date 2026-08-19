using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionMetricsGateBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void EvolutionMetricsGateBoundary_Is_Declared()
    {
        Assert.True(EvolutionMetricsGateBoundary.MetricsGateBoundaryImplemented);
        Assert.True(EvolutionFoundationBoundary.MetricsGateBoundaryImplemented);
        Assert.False(EvolutionMetricsGateBoundary.BiDashboardProductImplemented);
    }

    [Fact]
    public void PostP29_Evidence_Records_T003_And_MetricsGate_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T003", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionMetricsGateBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R1", plan, StringComparison.Ordinal);
    }
}
