namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R1: TripPlanner is the independent travel-intent / lead-submission owner (schema <c>trip_planner</c>).
/// Not Booking, Payment, Pricing, CRM, Search, Destination, Tour, Place, AgencyMarketplace,
/// Notification delivery, or Party/Identity master data.
/// </summary>
public static class TripPlannerOwnershipBoundary
{
    public const string OwnerModule = "TripPlanner";
    public const string SchemaName = "trip_planner";
    public const string DestinationOwner = "Destination";
    public const string ReferenceDataOwner = "ReferenceData";
    public const string TourOwner = "Tour";
    public const string PlaceOwner = "Place";
    public const string PricingOwner = "Pricing";
    public const string AgencyMarketplaceOwner = "AgencyMarketplace";
    public const string SearchOwner = "Search";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string NotificationOwner = "Notification";
    public const string IdentityOwner = "Identity";
    public const string PartyOwner = "Party";
    public const string CrmOwner = "Crm";
    public const string PresentationOwner = "PublicExperience";
    public const string LogicalReferencePosture = "OpaqueLogicalReferenceId";
    public const bool OwnsDestinationFacts = false;
    public const bool OwnsReferenceData = false;
    public const bool OwnsTourFacts = false;
    public const bool OwnsPlaceFacts = false;
    public const bool OwnsPricing = false;
    public const bool OwnsQuote = false;
    public const bool OwnsBooking = false;
    public const bool OwnsPayment = false;
    public const bool OwnsCrm = false;
    public const bool OwnsSearch = false;
    public const bool OwnsAgencyMarketplace = false;
    public const bool OwnsNotificationDelivery = false;
    public const bool OwnsIdentityOrParty = false;
    public const bool ProductReferencesAreLogicalOnly = true;
    public const bool ProductReferencesAreSourceOfTruth = false;
    public const bool TripIntentImplemented = true;
    public const bool LeadImplemented = true;
    public const bool AnonymousTripIntentSupported = true;
    public const bool AuthenticatedAssociationOptional = true;
    public const bool IdentityOrPartyCloneImplemented = false;
    public const bool TravelPreferencesImplemented = true;
    public const bool LeadContactSnapshotImplemented = true;
    public const bool LeadLifecycleImplemented = false;
    public const bool AgencyRoutingImplemented = false;
    public const bool ConsentModelImplemented = false;
    public const bool NotificationProviderImplemented = false;
    public const bool SearchEngineImplemented = false;
    public const bool RecommendationEngineImplemented = false;
    public const bool AiInfrastructureImplemented = false;
    public const bool GenericWorkflowEngineImplemented = false;
}
