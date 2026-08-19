namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R1: Analytics is the independent downstream product-analytics owner (schema <c>analytics</c>).
/// Search/Booking/Payment/Tour modules may publish semantic events; Analytics owns taxonomy/dispatch later.
/// Observability platform telemetry remains separate from product analytics SoR.
/// </summary>
public static class AnalyticsOwnershipBoundary
{
    public const string OwnerModule = "Analytics";
    public const string SchemaName = "analytics";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";

    public const string AnalyticsIsNotIdentity = "Analytics != Identity";
    public const string AnalyticsIsNotAccess = "Analytics != Access";
    public const string AnalyticsIsNotParty = "Analytics != Party";
    public const string AnalyticsIsNotBooking = "Analytics != Booking";
    public const string AnalyticsIsNotPayment = "Analytics != Payment";
    public const string AnalyticsIsNotSearch = "Analytics != Search";
    public const string AnalyticsIsNotSeo = "Analytics != SEO";
    public const string AnalyticsIsNotContent = "Analytics != Content";
    public const string AnalyticsIsNotDestination = "Analytics != Destination";
    public const string AnalyticsIsNotNotification = "Analytics != Notification";
    public const string AnalyticsIsNotObservability = "Analytics != Observability";
    public const string AnalyticsIsNotTripPlanner = "Analytics != TripPlanner";
    public const string AnalyticsIsNotB2B = "Analytics != B2B";
    public const string ProductAnalyticsIsNotPlatformTelemetry =
        "ProductAnalytics != PlatformTelemetry";
    public const string DownstreamConsumerPosture =
        "Analytics is downstream; core domain correctness must not depend synchronously on analytics dispatch success";

    public const string SearchOwner = "Search";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string NotificationOwner = "Notification";
    public const string ObservabilityOwner = "Observability";
    public const string IdentityOwner = "Identity";
    public const string AccessOwner = "Access";
    public const string PartyOwner = "Party";
    public const string TripPlannerOwner = "TripPlanner";
    public const string B2BOwner = "B2B";

    public const bool OwnsIdentityCredentials = false;
    public const bool OwnsAccessAuthorization = false;
    public const bool OwnsPartyIdentity = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool OwnsSearchRanking = false;
    public const bool OwnsSeoEditorial = false;
    public const bool OwnsContentEditorial = false;
    public const bool OwnsNotificationDelivery = false;
    public const bool OwnsPlatformTelemetry = false;
    public const bool OwnsTripPlannerFacts = false;
    public const bool OwnsB2BCommerce = false;
    public const bool SeparateAnalyticsModuleImplemented = true;
    public const bool SeparateAnalyticsSchemaImplemented = true;
    public const bool EventTaxonomyBoundaryImplemented = true;
    public const bool ProviderPortImplemented = true;
    public const bool ProviderAbstractionImplemented = true;
    public const bool ProviderImplemented = false;
    public const bool IngestionBoundaryImplemented = true;
    public const bool PreferenceBoundaryImplemented = true;
    public const bool OperationalBoundaryImplemented = true;
    public const bool DeferredScopeBoundaryImplemented = true;
    public const bool HardeningGuardrailsImplemented = true;
    public const bool EventPersistenceImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool ProductTablesImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool BookingPersistenceDependencyImplemented = false;
    public const bool PaymentPersistenceDependencyImplemented = false;
    public const bool ModifiesPaymentTargets = false;
}
