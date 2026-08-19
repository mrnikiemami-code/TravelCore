using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T008: operational hardening and deferred performance scope consolidating accepted P28 boundaries.
/// </summary>
public sealed class PerformanceHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Performance_Hardening_Guardrails_Are_Declared()
    {
        Assert.True(PerformanceOwnershipBoundary.HardeningGuardrailsImplemented);
        Assert.True(PerformanceOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(PerformanceDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
    }

    [Fact]
    public void Performance_T008_Locks_Accepted_Boundaries_T002_Through_T007()
    {
        Assert.True(PerformanceFoundationBoundary.SeparatePerformanceFoundationImplemented);
        Assert.True(PerformanceFoundationBoundary.MeasurementBoundaryImplemented);
        Assert.True(PerformanceFoundationBoundary.RuntimeBoundaryImplemented);
        Assert.True(PerformanceFoundationBoundary.DataAccessBoundaryImplemented);
        Assert.True(PerformanceFoundationBoundary.CacheBoundaryImplemented);
        Assert.True(PerformanceFoundationBoundary.ScalingBoundaryImplemented);
        Assert.True(PerformanceMeasurementBoundary.MeasurementBoundaryImplemented);
        Assert.True(PerformanceRuntimeBoundary.RuntimeBoundaryImplemented);
        Assert.True(PerformanceDataAccessBoundary.DataAccessBoundaryImplemented);
        Assert.True(PerformanceCacheBoundary.CacheBoundaryImplemented);
        Assert.True(PerformanceScalingBoundary.ScalingBoundaryImplemented);
    }

    [Fact]
    public void PerformanceOperationalBoundary_Forbids_Fake_Benchmarks_And_Optimization_Product()
    {
        Assert.Equal("Fake benchmark claims are NOT ALLOWED", PerformanceOperationalBoundary.NoFakeBenchmarkClaims);
        Assert.Equal("No production optimization product in T008", PerformanceOperationalBoundary.NoProductionOptimizationProduct);
        Assert.False(PerformanceOperationalBoundary.BenchmarkHarnessProductImplemented);
        Assert.False(PerformanceOperationalBoundary.ProductionTuningProductImplemented);
        Assert.False(PerformanceMeasurementBoundary.BenchmarkHarnessProductImplemented);
    }

    [Fact]
    public void PerformanceDeferredScopeBoundary_Keeps_Cdn_Frontend_Search_And_LoadTest_Deferred()
    {
        Assert.Equal("DEFERRED", PerformanceDeferredScopeBoundary.ProductionCdnVendorLockIn);
        Assert.Equal("DEFERRED", PerformanceDeferredScopeBoundary.FrontendBundleOptimizationPlatform);
        Assert.Equal("DEFERRED", PerformanceDeferredScopeBoundary.SearchRankingEngine);
        Assert.Equal("DEFERRED", PerformanceDeferredScopeBoundary.LoadTestInfrastructure);
        Assert.Equal("DEFERRED", PerformanceDeferredScopeBoundary.WebPAvifConversionPipeline);
        Assert.Equal("BOUNDARY DECLARED", PerformanceDeferredScopeBoundary.CdnStaticDeliveryPosture);
        Assert.Equal("Search read latency posture; ranking engine DEFERRED", PerformanceDeferredScopeBoundary.SearchReadPerformancePosture);
        Assert.False(PerformanceDeferredScopeBoundary.CdnVendorProductImplemented);
        Assert.False(PerformanceDeferredScopeBoundary.SearchEngineProductImplemented);
    }

    [Fact]
    public void Performance_T008_Forbids_Deferred_And_Ops_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(CdnVendorAdapter|FrontendBundleOptimizer|SearchRankingEngine|LoadTestHarness|BenchmarkReportPublisher|PerformanceAdminController|PublicPerformanceController|ProductionOptimizationService)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("///", StringComparison.Ordinal)
                        && forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Performance T008 forbids deferred/ops product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P28_Evidence_Records_T008_And_R5_R6_R7()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T008", plan, StringComparison.Ordinal);
        Assert.Contains("P28-R5", plan, StringComparison.Ordinal);
        Assert.Contains("P28-R6", plan, StringComparison.Ordinal);
        Assert.Contains("P28-R7", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceOperationalBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceDeferredScopeBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("operational hardening", plan, StringComparison.OrdinalIgnoreCase);
    }
}
