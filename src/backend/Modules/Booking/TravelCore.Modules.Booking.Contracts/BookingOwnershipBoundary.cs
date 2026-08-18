namespace TravelCore.Modules.Booking.Contracts;

/// <summary>
/// P19-R1: Booking is the independent Tour reservation owner (schema <c>booking</c>).
/// Initial logical target is TourDeparture. Capacity definition remains Tour-owned.
/// Capacity consumption ownership is Booking-owned; implementation is deferred to P19-R3.
/// Not Tour catalog, Pricing, Payment, Party/Identity master, AgencyMarketplace,
/// Search, SEO, Notification delivery, VisaApplication, or TripPlanner Lead.
/// </summary>
public static class BookingOwnershipBoundary
{
    public const string OwnerModule = "Booking";
    public const string SchemaName = "booking";
    public const string InitialTarget = "TourDeparture";
    public const string TourOwner = "Tour";
    public const string PricingOwner = "Pricing";
    public const string PaymentOwner = "Payment";
    public const string PartyOwner = "Party";
    public const string IdentityOwner = "Identity";
    public const string AgencyMarketplaceOwner = "AgencyMarketplace";
    public const string SearchOwner = "Search";
    public const string SeoOwner = "Seo";
    public const string NotificationOwner = "Notification";
    public const string VisaOwner = "Visa";
    public const string TripPlannerOwner = "TripPlanner";
    public const string PresentationOwner = "PublicExperience";
    public const string LogicalReferencePosture = "OpaqueLogicalReferenceId";
    public const bool OwnsTourCatalog = false;
    public const bool OwnsTourDeparture = false;
    public const bool OwnsCapacityDefinition = false;
    public const bool OwnsCapacityConsumption = true;
    public const bool CapacityConsumptionImplemented = false;
    public const bool OwnsPricing = false;
    public const bool OwnsQuote = false;
    public const bool OwnsPayment = false;
    public const bool OwnsPartyOrIdentity = false;
    public const bool OwnsAgencyMarketplace = false;
    public const bool OwnsSearch = false;
    public const bool OwnsSeo = false;
    public const bool OwnsNotificationDelivery = false;
    public const bool OwnsVisaApplication = false;
    public const bool OwnsTripPlannerLead = false;
    public const bool ProductReferencesAreLogicalOnly = true;
    public const bool ProductReferencesAreSourceOfTruth = false;
    public const bool BookingAggregateImplemented = false;
    public const bool BookingStatusImplemented = false;
    public const bool CapacityHoldImplemented = false;
    public const bool BookingPassengerImplemented = false;
    public const bool ContactSnapshotImplemented = false;
    public const bool QuoteIntegrationImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
    public const bool PublicBookingSurfaceImplemented = false;
    public const bool SearchEngineImplemented = false;
    public const bool AiInfrastructureImplemented = false;
    public const bool GenericWorkflowEngineImplemented = false;
    public const bool NotificationProviderImplemented = false;
}
