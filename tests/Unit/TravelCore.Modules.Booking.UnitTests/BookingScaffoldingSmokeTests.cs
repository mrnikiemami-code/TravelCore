using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingScaffoldingSmokeTests
{
    [Fact]
    public void BookingContractsAssembly_IsLoadable()
    {
        var marker = typeof(BookingContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Booking.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Booking.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void BookingDomainAssembly_IsLoadable()
    {
        var marker = typeof(BookingDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Booking.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Peer_SoT_Out_Of_Booking()
    {
        Assert.Equal("Booking", BookingOwnershipBoundary.OwnerModule);
        Assert.Equal("booking", BookingOwnershipBoundary.SchemaName);
        Assert.Equal("TourDeparture", BookingOwnershipBoundary.InitialTarget);
        Assert.Equal("Tour", BookingOwnershipBoundary.TourOwner);
        Assert.Equal("Pricing", BookingOwnershipBoundary.PricingOwner);
        Assert.Equal("Payment", BookingOwnershipBoundary.PaymentOwner);
        Assert.Equal("OpaqueLogicalReferenceId", BookingOwnershipBoundary.LogicalReferencePosture);
        Assert.False(BookingOwnershipBoundary.OwnsTourCatalog);
        Assert.False(BookingOwnershipBoundary.OwnsTourDeparture);
        Assert.False(BookingOwnershipBoundary.OwnsCapacityDefinition);
        Assert.True(BookingOwnershipBoundary.OwnsCapacityConsumption);
        Assert.True(BookingOwnershipBoundary.CapacityConsumptionImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.False(BookingOwnershipBoundary.OwnsQuote);
        Assert.False(BookingOwnershipBoundary.OwnsPayment);
        Assert.False(BookingOwnershipBoundary.OwnsPartyOrIdentity);
        Assert.False(BookingOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.False(BookingOwnershipBoundary.OwnsSearch);
        Assert.False(BookingOwnershipBoundary.OwnsSeo);
        Assert.False(BookingOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(BookingOwnershipBoundary.OwnsVisaApplication);
        Assert.False(BookingOwnershipBoundary.OwnsTripPlannerLead);
        Assert.True(BookingOwnershipBoundary.ProductReferencesAreLogicalOnly);
        Assert.False(BookingOwnershipBoundary.ProductReferencesAreSourceOfTruth);
        Assert.True(BookingOwnershipBoundary.BookingAggregateImplemented);
        Assert.True(BookingOwnershipBoundary.BookingStatusImplemented);
        Assert.True(BookingOwnershipBoundary.CapacityHoldImplemented);
        Assert.False(BookingOwnershipBoundary.BookingPassengerImplemented);
        Assert.False(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.False(BookingOwnershipBoundary.SearchEngineImplemented);
        Assert.False(BookingOwnershipBoundary.AiInfrastructureImplemented);
        Assert.False(BookingOwnershipBoundary.GenericWorkflowEngineImplemented);
        Assert.False(BookingOwnershipBoundary.NotificationProviderImplemented);
    }

    [Fact]
    public void TourDepartureReference_Is_Opaque_Logical_Id_Not_A_Departure_Entity()
    {
        var logicalId = Guid.Parse("0198b3e0-0000-7000-8000-000000000019");
        var reference = new TourDepartureReference(logicalId);
        Assert.Equal(logicalId, reference.LogicalId);
        Assert.Equal("TourDepartureReference", nameof(TourDepartureReference));
        Assert.False(typeof(TourDepartureReference).IsClass);
    }

    [Fact]
    public void BookingDbContext_Owns_Schema_booking()
    {
        Assert.Equal("booking", BookingDbContext.SchemaName);
        Assert.Equal(BookingOwnershipBoundary.SchemaName, BookingDbContext.SchemaName);
    }
}
