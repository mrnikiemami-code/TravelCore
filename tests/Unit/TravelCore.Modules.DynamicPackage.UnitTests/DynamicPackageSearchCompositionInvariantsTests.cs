using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageSearchCompositionInvariantsTests
{
    [Fact]
    public void TransientPackageCandidate_Create_Requires_FlightComponent()
    {
        var hotelComponent = HotelBookingId.New();
        var flightComponent = default(FlightBookingId);

        Assert.Throws<ArgumentException>(() =>
            TransientPackageCandidate.Create(flightComponent, hotelComponent));
    }

    [Fact]
    public void TransientPackageCandidate_Create_Requires_HotelComponent()
    {
        var flightComponent = FlightBookingId.New();
        var hotelComponent = default(HotelBookingId);

        Assert.Throws<ArgumentException>(() =>
            TransientPackageCandidate.Create(flightComponent, hotelComponent));
    }

    [Fact]
    public void TransientPackageCandidate_Create_Stores_Both_Component_References()
    {
        var flightComponent = FlightBookingId.New();
        var hotelComponent = HotelBookingId.New();

        var candidate = TransientPackageCandidate.Create(flightComponent, hotelComponent);

        Assert.Equal(flightComponent, candidate.FlightComponent);
        Assert.Equal(hotelComponent, candidate.HotelComponent);
    }
}

