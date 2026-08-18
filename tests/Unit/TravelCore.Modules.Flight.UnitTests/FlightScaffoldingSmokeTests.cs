using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightScaffoldingSmokeTests
{
    [Fact]
    public void FlightContractsAssembly_IsLoadable()
    {
        var marker = typeof(FlightContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Flight.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Flight.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void FlightDomainAssembly_IsLoadable()
    {
        var marker = typeof(FlightDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Flight.Domain", marker.Namespace);
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightBooking"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Flight.Domain.BookingBase"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Flight.Domain.GenericBookingAggregate"));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T001_Foundation_Only()
    {
        Assert.Equal("Flight", FlightOwnershipBoundary.OwnerModule);
        Assert.Equal("flight", FlightOwnershipBoundary.SchemaName);
        Assert.Equal("Flight", FlightOwnershipBoundary.TransactionAggregateOwner);
        Assert.Equal("FlightBooking", FlightOwnershipBoundary.TransactionAggregateName);
        Assert.Equal("NONE", FlightOwnershipBoundary.NamedFlightSupplier);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionAvailabilitySource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionRateSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionReservationSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionTicketingSource);
        Assert.Equal("Flight != Tour", FlightOwnershipBoundary.FlightIsNotTour);
        Assert.Equal("FlightBooking != Tour Booking", FlightOwnershipBoundary.FlightBookingIsNotTourBooking);
        Assert.Equal("FlightBooking != HotelBooking", FlightOwnershipBoundary.FlightBookingIsNotHotelBooking);
        Assert.Equal("Tour Package Flight != live Flight inventory", FlightOwnershipBoundary.TourPackageFlightIsNotLiveInventory);
        Assert.Equal("TourDepartureTransportSegment", FlightOwnershipBoundary.TourTransportType);
        Assert.Equal("Tour", FlightOwnershipBoundary.TourTransportOwner);
        Assert.False(FlightOwnershipBoundary.OwnsTourPackageTransport);
        Assert.False(FlightOwnershipBoundary.OwnsTourBooking);
        Assert.False(FlightOwnershipBoundary.OwnsHotelBooking);
        Assert.False(FlightOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.False(FlightOwnershipBoundary.SeparateFlightBookingModuleImplemented);
        Assert.False(FlightOwnershipBoundary.SeparateFlightBookingSchemaImplemented);
        Assert.False(FlightOwnershipBoundary.FlightBookingAggregateImplemented);
        Assert.False(FlightOwnershipBoundary.ItineraryModelImplemented);
        Assert.False(FlightOwnershipBoundary.PassengerModelImplemented);
        Assert.False(FlightOwnershipBoundary.SearchModelImplemented);
        Assert.False(FlightOwnershipBoundary.OfferModelImplemented);
        Assert.False(FlightOwnershipBoundary.PnrModelImplemented);
        Assert.False(FlightOwnershipBoundary.TicketModelImplemented);
        Assert.False(FlightOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(FlightOwnershipBoundary.PublicApiImplemented);
        Assert.False(FlightOwnershipBoundary.FrontendImplemented);
        Assert.False(FlightOwnershipBoundary.SupplierSdkImplemented);
        Assert.False(FlightOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(FlightOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(FlightOwnershipBoundary.ProductTablesImplemented);
    }

    [Fact]
    public void FlightDbContext_Owns_Schema_flight()
    {
        Assert.Equal("flight", FlightDbContext.SchemaName);
        Assert.Equal(FlightOwnershipBoundary.SchemaName, FlightDbContext.SchemaName);
    }
}
