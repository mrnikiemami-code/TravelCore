namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// P25-R5: delivery preferences are distinct from TripPlanner consent snapshots.
/// </summary>
public static class NotificationConsentInteractionBoundary
{
    public const string DeliveryPreferenceNotEqualTripPlannerConsentSnapshot =
        "DeliveryPreference != TripPlannerConsentSnapshot";
    public const string MarketingVsTransactionalSeparationPreserved =
        "Marketing vs transactional separation preserved";
    public const string TripPlannerConsentSnapshotOwner = "TripPlanner";
    public const string NotificationPreferenceOwner = "Notification";

    public const bool PreferenceBoundaryImplemented = true;
    public const bool PreferencePersistenceImplemented = false;
    public const bool TripPlannerConsentOwnershipTransferred = false;
}
