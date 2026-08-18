namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// P21-R1: HotelBooking is the independent hotel-reservation transaction owner
/// (schema <c>hotel_booking</c>). Place remains hotel/accommodation catalog authority.
/// HotelBooking references Place only through an opaque logical PlaceId.
/// </summary>
public static class HotelBookingOwnershipBoundary
{
    public const string OwnerModule = "HotelBooking";
    public const string SchemaName = "hotel_booking";
    public const string CatalogOwner = "Place";
    public const string CatalogIdentity = "PlaceId";
    public const string LogicalReferencePosture = "OpaqueLogicalPlaceId";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";
    public const string NamedHotelSupplier = "NONE";
    public const string ProviderSecretPosture = "SecureConfigurationNotSourceControl";

    public const string HotelBookingIsNotPlace = "HotelBooking != Place";
    public const string HotelBookingIsNotHotelCatalog = "HotelBooking != Hotel Catalog";
    public const string HotelBookingIsNotTourBooking = "HotelBooking != Tour Booking";
    public const string HotelCatalogOwnerIsPlace = "Place = hotel/accommodation catalog truth";
    public const string HotelBookingOwnsReservationTruth = "HotelBooking = hotel reservation transaction truth";

    public const bool OwnsPlaceCatalog = false;
    public const bool OwnsHotelName = false;
    public const bool OwnsHotelDescription = false;
    public const bool OwnsAmenities = false;
    public const bool OwnsAddress = false;
    public const bool OwnsDestination = false;
    public const bool OwnsMedia = false;
    public const bool OwnsHotelSeoEditorial = false;
    public const bool OwnsTourBooking = false;
    public const bool OwnsPayment = false;
    public const bool OwnsPricing = false;
    public const bool GenericBookingAbstractionImplemented = false;
    public const bool HotelBookingAggregateImplemented = true;
    public const bool HotelBookingStatusImplemented = true;
    public const bool RoomModelImplemented = true;
    public const bool GuestModelImplemented = true;
    public const bool AvailabilityHoldModelImplemented = true;
    public const bool SupplierAdapterImplemented = false;
    public const bool SupplierSdkImplemented = false;
    public const bool RateQuoteModelImplemented = true;
    public const bool CancellationModelImplemented = true;
    public const bool PaymentIntegrationImplemented = true;
    public const bool HotelBookingApiImplemented = false;
    public const bool HotelBookingUiImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool PlacePersistenceDependencyImplemented = false;
    public const bool ProductReferencesAreLogicalOnly = true;
    public const bool ProductReferencesAreSourceOfTruth = false;
}
