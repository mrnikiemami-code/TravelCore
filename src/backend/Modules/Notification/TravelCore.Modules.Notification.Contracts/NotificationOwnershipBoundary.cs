namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// P25-R1: Notification is the independent downstream delivery owner (schema <c>notification</c>).
/// Booking/Payment/TripPlanner may publish semantic events; Notification owns channel/provider orchestration later.
/// </summary>
public static class NotificationOwnershipBoundary
{
    public const string OwnerModule = "Notification";
    public const string SchemaName = "notification";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";

    public const string NotificationIsNotIdentity = "Notification != Identity";
    public const string NotificationIsNotAccess = "Notification != Access";
    public const string NotificationIsNotParty = "Notification != Party";
    public const string NotificationIsNotBooking = "Notification != Booking";
    public const string NotificationIsNotPayment = "Notification != Payment";
    public const string NotificationIsNotTripPlanner = "Notification != TripPlanner";
    public const string NotificationIsNotB2B = "Notification != B2B";
    public const string NotificationIntentNotEqualNotificationDelivery = "NotificationIntent != NotificationDelivery";
    public const string DownstreamConsumerPosture =
        "Notification is downstream; core domain correctness must not depend synchronously on delivery success";

    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string TripPlannerOwner = "TripPlanner";
    public const string IdentityOwner = "Identity";
    public const string AccessOwner = "Access";
    public const string PartyOwner = "Party";
    public const string B2BOwner = "B2B";

    public const bool OwnsIdentityCredentials = false;
    public const bool OwnsAccessAuthorization = false;
    public const bool OwnsPartyIdentity = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool OwnsTripPlannerFacts = false;
    public const bool OwnsB2BCommerce = false;
    public const bool SeparateNotificationModuleImplemented = true;
    public const bool SeparateNotificationSchemaImplemented = true;
    public const bool ChannelBoundaryImplemented = true;
    public const bool ProviderPortImplemented = true;
    public const bool ProviderAbstractionImplemented = true;
    public const bool ProviderImplemented = false;
    public const bool ChannelPersistenceImplemented = false;
    public const bool TemplateOrchestrationImplemented = false;
    public const bool DeliveryStatePersistenceImplemented = false;
    public const bool PreferencePersistenceImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool ProductTablesImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool BookingPersistenceDependencyImplemented = false;
    public const bool PaymentPersistenceDependencyImplemented = false;
    public const bool ModifiesPaymentTargets = false;
}
