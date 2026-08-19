using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.TripPlanner.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P27-T008: hardening guardrails consolidating accepted P27 boundaries and resolving R5/R7/R8 posture.
/// </summary>
public sealed class AnalyticsHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Analytics_Hardening_Guardrails_Are_Declared()
    {
        Assert.True(AnalyticsOwnershipBoundary.HardeningGuardrailsImplemented);
        Assert.True(AnalyticsConsentInteractionBoundary.ConsentInteractionBoundaryImplemented);
        Assert.True(AnalyticsOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(AnalyticsDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
    }

    [Fact]
    public void Analytics_T008_Locks_Accepted_Boundaries_T004_Through_T007()
    {
        Assert.True(AnalyticsOwnershipBoundary.SeparateAnalyticsModuleImplemented);
        Assert.True(AnalyticsOwnershipBoundary.EventTaxonomyBoundaryImplemented);
        Assert.True(AnalyticsOwnershipBoundary.ProviderPortImplemented);
        Assert.True(AnalyticsOwnershipBoundary.ProviderAbstractionImplemented);
        Assert.True(AnalyticsOwnershipBoundary.IngestionBoundaryImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProviderImplemented);
        Assert.False(AnalyticsOwnershipBoundary.PublicApiImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProductTablesImplemented);
        Assert.False(AnalyticsOwnershipBoundary.EventPersistenceImplemented);
    }

    [Fact]
    public void AnalyticsConsentInteractionBoundary_Keeps_Consent_Ownership_In_TripPlanner()
    {
        Assert.Equal("TripPlanner", AnalyticsConsentInteractionBoundary.TripPlannerConsentSnapshotOwner);
        Assert.Equal("Notification", AnalyticsConsentInteractionBoundary.NotificationPreferenceOwner);
        Assert.Equal("Analytics", AnalyticsConsentInteractionBoundary.AnalyticsConsentOwner);
        Assert.False(AnalyticsConsentInteractionBoundary.TripPlannerConsentOwnershipTransferred);
        Assert.False(AnalyticsConsentInteractionBoundary.NotificationPreferenceOwnershipTransferred);
        Assert.False(AnalyticsAttributionBoundary.OverwritesTripPlannerConsentSnapshots);
        Assert.False(AnalyticsAttributionBoundary.OverwritesNotificationDeliveryPreferences);
        Assert.True(TripPlannerConsentBoundary.ConsentModelImplemented);
        Assert.True(NotificationConsentInteractionBoundary.PreferenceBoundaryImplemented);
    }

    [Fact]
    public void AnalyticsOperationalBoundary_Forbids_Fake_Dispatch_Success_And_Public_Surface()
    {
        Assert.Equal("NOT ALLOWED", AnalyticsOperationalBoundary.FakeProductionDispatchSuccess);
        Assert.False(AnalyticsOperationalBoundary.FakeDispatchSuccessImplemented);
        Assert.False(AnalyticsOperationalBoundary.PublicApiImplemented);
        Assert.False(AnalyticsOperationalBoundary.AdminApiImplemented);
    }

    [Fact]
    public void AnalyticsDeferredScopeBoundary_Keeps_Warehouse_Bi_Ml_Streaming_Deferred()
    {
        Assert.Equal("DEFERRED", AnalyticsDeferredScopeBoundary.DataWarehouse);
        Assert.Equal("DEFERRED", AnalyticsDeferredScopeBoundary.BiDashboards);
        Assert.Equal("DEFERRED", AnalyticsDeferredScopeBoundary.MlRecommendation);
        Assert.Equal("DEFERRED", AnalyticsDeferredScopeBoundary.RealTimeStreamingAnalytics);
        Assert.Equal("DEFERRED", AnalyticsDeferredScopeBoundary.CrossVendorIdentityGraph);
        Assert.False(AnalyticsDeferredScopeBoundary.WarehouseConnectorImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.BiDashboardImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.IdentityGraphImplemented);
    }

    [Fact]
    public void Analytics_T008_Forbids_Deferred_And_Public_Ops_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(DataWarehouseConnector|BiDashboard|AnalyticsWarehouse|StreamingPipeline|IdentityGraphService|MlRecommendationEngine|AnalyticsAdminController|PublicAnalyticsController|FakeDispatchSuccessService)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Analytics T008 forbids deferred/public-ops types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P27_Evidence_Records_T008_And_R5_R7_R8()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P27-implementation-plan.md"));
        Assert.Contains("TC-P27-T008", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R5", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R7", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R8", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsConsentInteractionBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsOperationalBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsDeferredScopeBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("hardening and guardrails", plan, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void P27_Evidence_Pack_Locks_T009_Hardening_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P27-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TC-P27-T009",
            "P27-R1",
            "P27-R8",
            "Analytics != Booking/Payment",
            "Named Provider = NONE",
            "FailedDispatch != SourceOfRecordRollback",
            "READY_FOR_GATE",
            "TC-P27-GATE",
            "NOT EXECUTED",
        ];

        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
