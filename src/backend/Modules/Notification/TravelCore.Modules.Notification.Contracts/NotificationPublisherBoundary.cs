namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// P25-R1 publisher vs delivery-owner posture. Business modules emit semantic facts/events only.
/// </summary>
public static class NotificationPublisherBoundary
{
    public const string SemanticEventPublisherPosture =
        "Business modules publish semantic events; Notification owns delivery orchestration";

    public const string BookingPublisherOwner = "Booking";
    public const string PaymentPublisherOwner = "Payment";
    public const string TripPlannerPublisherOwner = "TripPlanner";
    public const string DeliveryOwner = "Notification";
}
