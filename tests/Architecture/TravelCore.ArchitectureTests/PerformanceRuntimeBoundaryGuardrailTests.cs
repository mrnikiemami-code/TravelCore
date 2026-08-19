using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T004: runtime performance boundary and module interaction model without infrastructure product.
/// </summary>
public sealed class PerformanceRuntimeBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PerformanceRuntimeBoundary_Is_Declared()
    {
        Assert.True(PerformanceRuntimeBoundary.RuntimeBoundaryImplemented);
        Assert.Equal(
            "Runtime performance decisions require measurement evidence",
            PerformanceRuntimeBoundary.MeasurementDrivenRuntime);
        Assert.Equal("No runtime tuning without measured evidence", PerformanceRuntimeBoundary.NoRuntimeTuningWithoutEvidence);
        Assert.True(PerformanceMeasurementBoundary.MeasurementBoundaryImplemented);
    }

    [Fact]
    public void PerformanceModuleInteractionBoundary_Preserves_Domain_Execution_Ownership()
    {
        Assert.True(PerformanceModuleInteractionBoundary.ModuleInteractionBoundaryImplemented);
        Assert.Equal(
            "Domain modules retain business execution ownership",
            PerformanceModuleInteractionBoundary.DomainModulesRetainExecutionOwnership);
        Assert.Equal("Performance != BookingExecution", PerformanceOwnershipBoundary.PerformanceIsNotBookingExecution);
        Assert.Equal("Performance != PaymentExecution", PerformanceOwnershipBoundary.PerformanceIsNotPaymentExecution);
        Assert.Equal("Performance != SearchRanking", PerformanceOwnershipBoundary.PerformanceIsNotSearchRanking);
        Assert.False(PerformanceModuleInteractionBoundary.DomainExecutionOwnershipTransferred);
        Assert.False(PerformanceModuleInteractionBoundary.CrossModulePerformanceHookImplemented);
    }

    [Fact]
    public void DomainModules_DoNot_Depend_On_Performance()
    {
        var forbidden = new[]
        {
            "TravelCore.Modules.Booking.Domain",
            "TravelCore.Modules.Booking.Infrastructure",
            "TravelCore.Modules.Payment.Domain",
            "TravelCore.Modules.Payment.Infrastructure",
            "TravelCore.Modules.Search.Domain",
            "TravelCore.Modules.Search.Infrastructure",
            "TravelCore.Modules.Analytics.Domain",
            "TravelCore.Modules.Analytics.Infrastructure",
        };

        var hits = forbidden
            .SelectMany(name =>
            {
                var project = Projects.SingleOrDefault(p => p.Name == name);
                if (project is null)
                {
                    return [];
                }

                return project.ProjectReferences
                    .Where(r => r.StartsWith("TravelCore.Performance", StringComparison.Ordinal))
                    .Select(r => $"{name} -> {r}");
            })
            .ToList();

        Assert.True(hits.Count == 0, "Domain modules must not reference TravelCore.Performance:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Performance_T004_Forbids_Runtime_Infrastructure_And_Hook_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(IRuntimeCacheHook|IRuntimeCdnHook|RuntimeDatabaseTuner|PerformanceInterceptor|BookingPerformanceHook|PaymentPerformanceHook|SearchPerformanceHook|PerformanceMiddleware|PerformanceHostedService)\b",
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
            "Performance T004 forbids runtime hook/infrastructure product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P28_Evidence_Records_T004_And_Runtime_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T004", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceRuntimeBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceModuleInteractionBoundary", plan, StringComparison.Ordinal);
    }
}
