using TravelCore.Identifiers;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingScaffoldingSmokeTests
{
    [Fact]
    public void HotelBookingContractsAssembly_IsLoadable()
    {
        var marker = typeof(HotelBookingContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.HotelBooking.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.HotelBooking.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void HotelBookingDomainAssembly_IsLoadable()
    {
        var marker = typeof(HotelBookingDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.HotelBooking.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Catalog_And_Peers_Out_Of_HotelBooking()
    {
        Assert.Equal("HotelBooking", HotelBookingOwnershipBoundary.OwnerModule);
        Assert.Equal("hotel_booking", HotelBookingOwnershipBoundary.SchemaName);
        Assert.Equal("Place", HotelBookingOwnershipBoundary.CatalogOwner);
        Assert.Equal("PlaceId", HotelBookingOwnershipBoundary.CatalogIdentity);
        Assert.Equal("OpaqueLogicalPlaceId", HotelBookingOwnershipBoundary.LogicalReferencePosture);
        Assert.Equal("UUIDv7", HotelBookingOwnershipBoundary.IdentityConvention);
        Assert.Equal("TravelCore.Money", HotelBookingOwnershipBoundary.MoneyModel);
        Assert.Equal("NodaTime", HotelBookingOwnershipBoundary.TemporalModel);
        Assert.Equal("NONE", HotelBookingOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("SecureConfigurationNotSourceControl", HotelBookingOwnershipBoundary.ProviderSecretPosture);
        Assert.Equal("HotelBooking != Place", HotelBookingOwnershipBoundary.HotelBookingIsNotPlace);
        Assert.Equal("HotelBooking != Hotel Catalog", HotelBookingOwnershipBoundary.HotelBookingIsNotHotelCatalog);
        Assert.Equal("HotelBooking != Tour Booking", HotelBookingOwnershipBoundary.HotelBookingIsNotTourBooking);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPlaceCatalog);
        Assert.False(HotelBookingOwnershipBoundary.OwnsTourBooking);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPayment);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPricing);
        Assert.False(HotelBookingOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingAggregateImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingStatusImplemented);
        Assert.True(HotelBookingOwnershipBoundary.RoomModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.GuestModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.AvailabilityHoldModelImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SupplierAdapterImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SupplierSdkImplemented);
        Assert.True(HotelBookingOwnershipBoundary.RateQuoteModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.CancellationModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingUiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PlacePersistenceDependencyImplemented);
        Assert.True(HotelBookingOwnershipBoundary.ProductReferencesAreLogicalOnly);
    }

    [Fact]
    public void HotelPlaceReference_Is_Opaque_Logical_Id_Not_A_Place_Entity()
    {
        var logicalId = Guid.Parse("0198b3e0-0000-7000-8000-000000000021");
        var reference = new HotelPlaceReference(logicalId);
        Assert.Equal(logicalId, reference.PlaceId);
        Assert.Equal("HotelPlaceReference", nameof(HotelPlaceReference));
        Assert.False(typeof(HotelPlaceReference).IsClass);
        Assert.Throws<ArgumentException>(() => new HotelPlaceReference(Guid.Empty));
    }

    [Fact]
    public void Future_HotelBooking_Identities_Use_Platform_Uuid7()
    {
        var id = Uuid7.New();
        Assert.Equal(7, id.Version);
    }

    [Fact]
    public void HotelBookingDbContext_Owns_Schema_hotel_booking()
    {
        Assert.Equal("hotel_booking", HotelBookingDbContext.SchemaName);
        Assert.Equal(HotelBookingOwnershipBoundary.SchemaName, HotelBookingDbContext.SchemaName);
    }

    [Fact]
    public void HotelBooking_T002_Has_Stay_Rooms_Guests_And_T005_Status()
    {
        var domain = typeof(HotelBookingDomainAssemblyMarker).Assembly;
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBooking"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.RoomReservation"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingGuest"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingStatus"));
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingAggregateImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingStatusImplemented);
        Assert.True(HotelBookingStayBoundary.MultiRoomSupported);
        Assert.False(HotelBookingStayBoundary.BirthDateStoredFlag);
        Assert.False(HotelBookingStayBoundary.PassportStored);
    }
}
