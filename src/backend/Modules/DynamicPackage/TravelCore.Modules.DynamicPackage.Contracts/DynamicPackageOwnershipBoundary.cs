namespace TravelCore.Modules.DynamicPackage.Contracts;

/// <summary>
/// P23-R1: DynamicPackage is the independent Flight + Hotel package orchestration owner
/// (schema <c>dynamic_package</c>). DynamicPackageBooking is owned inside DynamicPackage but is not implemented in T001.
/// FlightBooking remains Flight-owned. HotelBooking remains HotelBooking-owned. Payment execution remains Payment-owned.
/// </summary>
public static class DynamicPackageOwnershipBoundary
{
    public const string OwnerModule = "DynamicPackage";
    public const string SchemaName = "dynamic_package";
    public const string TransactionAggregateOwner = "DynamicPackage";
    public const string TransactionAggregateName = "DynamicPackageBooking";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";
    public const string ProductionCompositionSource = "NONE";
    public const string ProductionOrchestrationSource = "NONE";
    public const string ProviderSecretPosture = "SecureConfigurationNotSourceControl";

    public const string DynamicPackageIsNotTour = "DynamicPackage != Tour";
    public const string DynamicPackageIsNotTourBooking = "DynamicPackage != Tour Booking";
    public const string DynamicPackageIsNotFlight = "DynamicPackage != Flight";
    public const string DynamicPackageIsNotHotelBooking = "DynamicPackage != HotelBooking";
    public const string DynamicPackageBookingIsNotFlightBooking = "DynamicPackageBooking != FlightBooking";
    public const string DynamicPackageBookingIsNotHotelBooking = "DynamicPackageBooking != HotelBooking";
    public const string TourPackageFlightIsNotLiveInventory = "Tour Package Flight != live Flight inventory";
    public const string TourTransportType = "TourDepartureTransportSegment";
    public const string TourTransportOwner = "Tour";
    public const string FlightBookingOwner = "Flight";
    public const string HotelBookingOwner = "HotelBooking";
    public const string PaymentExecutionOwner = "Payment";

    public const bool OwnsTourPackageTransport = false;
    public const bool OwnsTourBooking = false;
    public const bool OwnsFlightBookingExecution = false;
    public const bool OwnsHotelBookingExecution = false;
    public const bool OwnsPlaceCatalog = false;
    public const bool OwnsPayment = false;
    public const bool OwnsPricing = false;
    public const bool OwnsSearchIndex = false;
    public const bool GenericBookingAbstractionImplemented = false;
    public const bool SeparateDynamicPackageModuleImplemented = true;
    public const bool SeparateDynamicPackageSchemaImplemented = true;
    public const bool DynamicPackageBookingAggregateImplemented = false;
    public const bool DynamicPackageBookingStatusImplemented = false;
    public const bool CompositionModelImplemented = true;
    public const bool PackageOfferModelImplemented = false;
    public const bool PackageMonetaryModelImplemented = false;
    public const bool OrchestrationModelImplemented = false;
    public const bool SagaModelImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
    public const bool CancellationModelImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
    public const bool SupplierSdkImplemented = false;
    public const bool SupplierAdapterImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool FlightPersistenceDependencyImplemented = false;
    public const bool HotelBookingPersistenceDependencyImplemented = false;
    public const bool PaymentPersistenceDependencyImplemented = false;
    public const bool ProductTablesImplemented = true;
}
