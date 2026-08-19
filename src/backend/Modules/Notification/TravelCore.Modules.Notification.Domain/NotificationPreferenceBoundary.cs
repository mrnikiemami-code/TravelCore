namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R5 preference boundary marker. Notification may own delivery preferences later; TripPlanner owns consent snapshots.
/// </summary>
public static class NotificationPreferenceBoundary
{
    public const string PreferenceOwner = "Notification";
    public const string ConsentSnapshotOwner = "TripPlanner";
    public const string MarketingChannelPosture = "Marketing vs transactional separation preserved";

    public const bool NotificationOwnsDeliveryPreferences = true;
    public const bool PreferencePersistenceImplemented = false;
    public const bool OverwritesTripPlannerConsentSnapshots = false;
}
