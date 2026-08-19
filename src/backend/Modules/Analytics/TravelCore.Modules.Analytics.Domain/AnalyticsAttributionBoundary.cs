namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R5 attribution boundary marker. Analytics may own attribution posture later; TripPlanner owns consent snapshots.
/// </summary>
public static class AnalyticsAttributionBoundary
{
    public const string AttributionOwner = "Analytics";
    public const string ConsentSnapshotOwner = "TripPlanner";
    public const string NotificationPreferenceOwner = "Notification";
    public const string MarketingVsProductAnalyticsSeparation =
        "Marketing vs product analytics separation preserved";

    public const bool AnalyticsOwnsAttributionPosture = true;
    public const bool AttributionPersistenceImplemented = false;
    public const bool OverwritesTripPlannerConsentSnapshots = false;
    public const bool OverwritesNotificationDeliveryPreferences = false;
}
