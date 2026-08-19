using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T003: measurement foundation and Observability interaction boundary without optimization product.
/// </summary>
public sealed class PerformanceMeasurementBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PerformanceMeasurementBoundary_Is_Declared()
    {
        Assert.True(PerformanceMeasurementBoundary.MeasurementBoundaryImplemented);
        Assert.Equal("Profile before optimize is mandatory", PerformanceMeasurementBoundary.ProfileBeforeOptimizeMandatory);
        Assert.Equal("No speculative tuning without measured evidence", PerformanceMeasurementBoundary.NoSpeculativeTuning);
        Assert.Equal(
            "No benchmark claims without evidence",
            PerformanceMeasurementBoundary.NoBenchmarkClaimsWithoutEvidence);
        Assert.True(PerformanceFoundationBoundary.MeasurementBoundaryImplemented);
    }

    [Fact]
    public void PerformanceObservabilityInteractionBoundary_Preserves_Observability_Ownership()
    {
        Assert.True(PerformanceObservabilityInteractionBoundary.ObservabilityInteractionBoundaryImplemented);
        Assert.Equal(
            "Observability owns platform telemetry",
            PerformanceObservabilityInteractionBoundary.ObservabilityOwnsPlatformTelemetry);
        Assert.Equal("Performance != Observability", PerformanceOwnershipBoundary.PerformanceIsNotObservability);
        Assert.Equal("Observability", PerformanceOwnershipBoundary.ObservabilityOwner);
        Assert.False(PerformanceObservabilityInteractionBoundary.ObservabilityProjectReferenceRequired);
        Assert.False(PerformanceObservabilityInteractionBoundary.ApmExporterImplemented);
        Assert.False(PerformanceObservabilityInteractionBoundary.OpenTelemetryProductImplemented);
    }

    [Fact]
    public void PerformanceModule_DoesNot_Reference_Observability_Or_Analytics()
    {
        var performance = Projects.Single(p => p.Name == "TravelCore.Performance");
        var hits = performance.ProjectReferences
            .Where(r => r.StartsWith("TravelCore.Observability", StringComparison.Ordinal)
                || r.StartsWith("TravelCore.Modules.Analytics", StringComparison.Ordinal))
            .ToList();
        Assert.True(hits.Count == 0, "Performance must not reference Observability/Analytics:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Performance_T003_Forbids_Apm_Benchmark_And_Tuning_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(OpenTelemetry|OtlpExporter|ApmClient|BenchmarkRunner|LoadTestHarness|ProductionTuningService|PerformanceProfiler|ApmVendorAdapter|BenchmarkReport|MeterRegistry)\b",
            RegexOptions.Compiled);

        var hits = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                var text = File.ReadAllText(path);
                return pattern.Matches(text).Select(m => $"{path}: {m.Value}");
            })
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Performance T003 forbids early measurement/APM product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P28_Evidence_Records_T003_And_Measurement_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T003", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceMeasurementBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceObservabilityInteractionBoundary", plan, StringComparison.Ordinal);
    }
}
