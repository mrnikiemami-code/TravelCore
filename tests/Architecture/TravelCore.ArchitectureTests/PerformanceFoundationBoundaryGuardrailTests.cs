using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T002: performance foundation boundary without Redis/cache/CDN product implementation.
/// </summary>
public sealed class PerformanceFoundationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PerformanceProject_Exists_With_Foundation_Boundaries()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Performance");
        Assert.Equal("LOCKED", PerformanceFoundationBoundary.ProfileBeforeOptimize);
        Assert.True(PerformanceFoundationBoundary.SeparatePerformanceFoundationImplemented);
        Assert.True(PerformanceOwnershipBoundary.FoundationBoundaryImplemented);
    }

    [Fact]
    public void PerformanceFoundationBoundary_Keeps_Redis_And_Cache_Non_SoR()
    {
        Assert.Equal("Redis != SourceOfRecord", PerformanceFoundationBoundary.RedisIsNotSourceOfRecord);
        Assert.Equal("Cache != SourceOfRecord", PerformanceFoundationBoundary.CacheIsNotSourceOfRecord);
        Assert.False(PerformanceFoundationBoundary.RedisClientImplemented);
        Assert.False(PerformanceFoundationBoundary.CachePolicyImplemented);
        Assert.False(PerformanceFoundationBoundary.CdnIntegrationImplemented);
    }

    [Fact]
    public void PerformanceOwnershipBoundary_Preserves_Module_Ownership()
    {
        Assert.Equal("Performance != Observability", PerformanceOwnershipBoundary.PerformanceIsNotObservability);
        Assert.Equal("Performance != ProductAnalytics", PerformanceOwnershipBoundary.PerformanceIsNotProductAnalytics);
        Assert.Equal("Performance != SearchRanking", PerformanceOwnershipBoundary.PerformanceIsNotSearchRanking);
        Assert.False(PerformanceOwnershipBoundary.OwnsPlatformTelemetry);
        Assert.False(PerformanceOwnershipBoundary.OwnsProductAnalytics);
        Assert.False(PerformanceOwnershipBoundary.OwnsSearchRanking);
    }

    [Fact]
    public void P28_Evidence_Records_T002_And_Foundation_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T002", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceFoundationBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceOwnershipBoundary", plan, StringComparison.Ordinal);
    }
}
