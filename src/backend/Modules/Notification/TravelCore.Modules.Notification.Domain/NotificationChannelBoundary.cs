namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R2 channel boundary marker. Taxonomy and ownership only — no delivery persistence or provider execution in T005.
/// </summary>
public static class NotificationChannelBoundary
{
    public const string ChannelTaxonomy = "Email · SMS · In-app";
    public const string PublisherDoesNotCallProviderDirectly = "Publishers do not call providers directly";
    public const string DeliveryOwner = "Notification";
    public const string BookingPublisherOwner = "Booking";
    public const string PaymentPublisherOwner = "Payment";
    public const string TripPlannerPublisherOwner = "TripPlanner";

    public const bool NotificationOwnsChannelTaxonomy = true;
    public const bool NotificationOwnsDeliveryState = false;
    public const bool ChannelPersistenceImplemented = false;
    public const bool ProviderExecutionImplemented = false;
    public const bool SmtpClientImplemented = false;
    public const bool TwilioImplemented = false;
    public const bool PushChannelImplemented = false;
    public const bool PublicApiImplemented = false;
}
