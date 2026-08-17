namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R7: Notification delivery ownership vs TripPlanner business intent markers.
/// </summary>
public static class TripPlannerNotificationBoundary
{
    public const string NotificationIntentNotEqualNotificationDelivery = "NotificationIntent != NotificationDelivery";
    public const string TripPlannerNotEqualNotificationProvider = "TripPlanner != Notification Provider";
    public const string LeadSubmittedAcknowledgementIntent = "LeadSubmittedAcknowledgement";
    public const string InternalLeadCreatedNotificationIntent = "InternalLeadCreatedNotification";
    public const bool NotificationProviderImplemented = false;
}
