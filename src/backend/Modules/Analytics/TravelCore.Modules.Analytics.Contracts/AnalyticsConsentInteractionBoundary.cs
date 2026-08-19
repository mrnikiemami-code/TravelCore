namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R5: analytics consent/attribution posture is distinct from TripPlanner consent snapshots and Notification delivery preferences.
/// </summary>
public static class AnalyticsConsentInteractionBoundary
{
    public const string AnalyticsConsentNotEqualTripPlannerConsentSnapshot =
        "AnalyticsConsent != TripPlannerConsentSnapshot";
    public const string AnalyticsAttributionNotEqualNotificationPreference =
        "AnalyticsAttribution != NotificationDeliveryPreference";
    public const string MarketingVsProductAnalyticsSeparationPreserved =
        "Marketing vs product analytics separation preserved";
    public const string TripPlannerConsentSnapshotOwner = "TripPlanner";
    public const string NotificationPreferenceOwner = "Notification";
    public const string AnalyticsConsentOwner = "Analytics";

    public const bool ConsentInteractionBoundaryImplemented = true;
    public const bool ConsentPersistenceImplemented = false;
    public const bool TripPlannerConsentOwnershipTransferred = false;
    public const bool NotificationPreferenceOwnershipTransferred = false;
}
