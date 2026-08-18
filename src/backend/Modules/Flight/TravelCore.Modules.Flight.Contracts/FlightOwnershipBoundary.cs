namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// P22-R1: Flight is the independent live-flight commerce / transaction owner
/// (schema <c>flight</c>). TourDepartureTransportSegment remains Tour-owned package transport.
/// FlightBooking is owned inside Flight. T004 adds immutable offer/monetary/fare-rule snapshots; PNR/Payment remain out of scope.
/// </summary>
public static class FlightOwnershipBoundary
{
    public const string OwnerModule = "Flight";
    public const string SchemaName = "flight";
    public const string TransactionAggregateOwner = "Flight";
    public const string TransactionAggregateName = "FlightBooking";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";
    public const string NamedFlightSupplier = "NONE";
    public const string ProductionSearchSource = "NONE";
    public const string ProductionAvailabilitySource = "NONE";
    public const string ProductionRateSource = "NONE";
    public const string ProductionOfferSource = "NONE";
    public const string ProductionReservationSource = "NONE";
    public const string ProductionTicketingSource = "NONE";
    public const string ProviderSecretPosture = "SecureConfigurationNotSourceControl";
    public const string InventoryAuthority = "external source-authoritative";
    public const string SearchSourcePort = "IFlightSearchSource";
    public const string AvailabilitySourcePort = "IFlightOfferAvailabilitySource";
    public const string OfferSourcePort = "IFlightOfferSource";
    public const string SourceCapabilities = "Search, AvailabilityCheck, OfferRevalidation";
    public const string AirportAuthorityStatus = "RESOLVED (P22-R2) ReferenceData";
    public const string AirlineAuthorityStatus = "RESOLVED (P22-R2) ReferenceData";
    public const string AirportCandidateOwner = "ReferenceData";
    public const string AirlineCandidateOwner = "ReferenceData";

    public const string FlightIsNotTour = "Flight != Tour";
    public const string FlightBookingIsNotTourBooking = "FlightBooking != Tour Booking";
    public const string FlightBookingIsNotHotelBooking = "FlightBooking != HotelBooking";
    public const string TourPackageFlightIsNotLiveInventory = "Tour Package Flight != live Flight inventory";
    public const string TourTransportType = "TourDepartureTransportSegment";
    public const string TourTransportOwner = "Tour";

    public const bool OwnsTourPackageTransport = false;
    public const bool OwnsTourBooking = false;
    public const bool OwnsHotelBooking = false;
    public const bool OwnsPlaceCatalog = false;
    public const bool OwnsAirportCatalog = false;
    public const bool OwnsAirlineCatalog = false;
    public const bool OwnsPayment = false;
    public const bool OwnsPricing = false;
    public const bool OwnsSearchIndex = false;
    public const bool GenericBookingAbstractionImplemented = false;
    public const bool SeparateFlightBookingModuleImplemented = false;
    public const bool SeparateFlightBookingSchemaImplemented = false;
    public const bool FlightBookingAggregateImplemented = true;
    public const bool FlightBookingStatusImplemented = false;
    public const bool ItineraryModelImplemented = true;
    public const bool SegmentModelImplemented = true;
    public const bool PassengerModelImplemented = true;
    public const bool SearchModelImplemented = true;
    public const bool AvailabilityModelImplemented = true;
    public const bool OfferModelImplemented = true;
    public const bool FareModelImplemented = true;
    public const bool PnrModelImplemented = false;
    public const bool TicketModelImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
    public const bool CancellationModelImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
    public const bool SupplierSdkImplemented = false;
    public const bool SupplierAdapterImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool TourPersistenceDependencyImplemented = false;
    public const bool ProductTablesImplemented = true;
}
