using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageCompositionInvariantsTests
{
    [Fact]
    public void PackageComposition_Create_Requires_FlightBookingId()
    {
        var hotelBookingId = HotelBookingId.New();
        var flightBookingId = default(FlightBookingId);

        Assert.Throws<ArgumentException>(() =>
            PackageComposition.Create(flightBookingId, hotelBookingId));
    }

    [Fact]
    public void PackageComposition_Create_Requires_HotelBookingId()
    {
        var flightBookingId = FlightBookingId.New();
        var hotelBookingId = default(HotelBookingId);

        Assert.Throws<ArgumentException>(() =>
            PackageComposition.Create(flightBookingId, hotelBookingId));
    }

    [Fact]
    public void PackageComposition_Create_Stores_Exactly_One_Reference_Each()
    {
        var flightBookingId = FlightBookingId.New();
        var hotelBookingId = HotelBookingId.New();

        var composition = PackageComposition.Create(flightBookingId, hotelBookingId);

        Assert.NotEqual(default, composition.Id.Value);
        Assert.Equal(flightBookingId, composition.FlightBookingId);
        Assert.Equal(hotelBookingId, composition.HotelBookingId);
    }
}

