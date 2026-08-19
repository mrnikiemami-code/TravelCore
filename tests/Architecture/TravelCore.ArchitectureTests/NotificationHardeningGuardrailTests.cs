using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Domain;
using TravelCore.Modules.TripPlanner.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P25-T008: hardening guardrails consolidating accepted P25 boundaries and resolving R5/R7/R8 posture.
/// </summary>
public sealed class NotificationHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Notification_Hardening_Guardrails_Are_Declared()
    {
        Assert.True(NotificationOwnershipBoundary.HardeningGuardrailsImplemented);
        Assert.True(NotificationConsentInteractionBoundary.PreferenceBoundaryImplemented);
        Assert.True(NotificationOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(NotificationDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
    }

    [Fact]
    public void Notification_T008_Locks_Accepted_Boundaries_T004_Through_T007()
    {
        Assert.True(NotificationOwnershipBoundary.SeparateNotificationModuleImplemented);
        Assert.True(NotificationOwnershipBoundary.ChannelBoundaryImplemented);
        Assert.True(NotificationOwnershipBoundary.ProviderPortImplemented);
        Assert.True(NotificationOwnershipBoundary.TemplateOrchestrationImplemented);
        Assert.True(NotificationOwnershipBoundary.EventConsumptionBoundaryImplemented);
        Assert.False(NotificationOwnershipBoundary.ProviderImplemented);
        Assert.False(NotificationOwnershipBoundary.PublicApiImplemented);
        Assert.False(NotificationOwnershipBoundary.ProductTablesImplemented);
        Assert.False(TripPlannerNotificationBoundary.NotificationProviderImplemented);
    }

    [Fact]
    public void NotificationPreferenceBoundary_Keeps_Consent_Ownership_In_TripPlanner()
    {
        Assert.Equal("TripPlanner", NotificationConsentInteractionBoundary.TripPlannerConsentSnapshotOwner);
        Assert.Equal("Notification", NotificationConsentInteractionBoundary.NotificationPreferenceOwner);
        Assert.False(NotificationConsentInteractionBoundary.TripPlannerConsentOwnershipTransferred);
        Assert.False(NotificationPreferenceBoundary.OverwritesTripPlannerConsentSnapshots);
        Assert.True(TripPlannerConsentBoundary.ConsentModelImplemented);
    }

    [Fact]
    public void NotificationOperationalBoundary_Forbids_Fake_Send_Success_And_Public_Surface()
    {
        Assert.Equal("NOT ALLOWED", NotificationOperationalBoundary.FakeProductionSendSuccess);
        Assert.False(NotificationOperationalBoundary.FakeSendSuccessImplemented);
        Assert.False(NotificationOperationalBoundary.PublicApiImplemented);
        Assert.False(NotificationOperationalBoundary.AdminApiImplemented);
    }

    [Fact]
    public void NotificationDeferredScopeBoundary_Keeps_Push_Webhook_Campaign_Deferred()
    {
        Assert.Equal("DEFERRED", NotificationDeferredScopeBoundary.PushNotifications);
        Assert.Equal("DEFERRED", NotificationDeferredScopeBoundary.WebhookDelivery);
        Assert.Equal("DEFERRED", NotificationDeferredScopeBoundary.MarketingCampaignPlatform);
        Assert.False(NotificationDeferredScopeBoundary.PushChannelImplemented);
        Assert.False(NotificationDeferredScopeBoundary.WebhookEndpointImplemented);
    }

    [Fact]
    public void Notification_T008_Forbids_Deferred_And_Public_Ops_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(PushProvider|WebhookEndpoint|CampaignPlatform|MarketingCampaign|NotificationAdminController|PublicNotificationController|FakeSendSuccessService)\b",
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

        Assert.True(hits.Count == 0, "Notification T008 forbids deferred/public-ops types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P25_Evidence_Records_T008_And_R5_R7_R8()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P25-implementation-plan.md"));
        Assert.Contains("TC-P25-T008", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R5", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R7", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R8", plan, StringComparison.Ordinal);
        Assert.Contains("hardening and guardrails", plan, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void P25_Gate_Evidence_Locks_Acceptance_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P25-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TC-P25-GATE",
            "P25 COMPLETE",
            "P25-R1",
            "P25-R8",
            "TC-P25-T009",
            "No new Notification product capability",
            "P26",
            "NOT IMPLEMENTED",
        ];

        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void P25_Evidence_Pack_Locks_T009_Hardening_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P25-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TC-P25-T009",
            "P25-R1",
            "P25-R8",
            "Notification != Booking/Payment",
            "Named Provider = NONE",
            "FailedDelivery != SourceOfRecordRollback",
            "READY_FOR_GATE",
            "TC-P25-GATE",
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
