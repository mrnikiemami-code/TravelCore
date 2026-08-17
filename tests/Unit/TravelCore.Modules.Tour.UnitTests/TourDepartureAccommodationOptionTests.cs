using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureAccommodationOptionTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 7, 0);
    private static readonly Guid PlaceId = Guid.Parse("01900000-0000-7000-8000-000000000901");

    private static TourDeparture NewDeparture()
    {
        var product = TourProduct.CreatePackage("PKG-ACC-001", "Stay Package", Now);
        return TourDeparture.Create(product, Now);
    }

    [Fact]
    public void AddAccommodationOption_Stores_Logical_Place_Nights_And_Board()
    {
        var departure = NewDeparture();
        var option = departure.AddAccommodationOption(
            PlaceId,
            5,
            TourDepartureBoardType.Breakfast,
            Now);

        Assert.Equal(PlaceId, option.PlaceId);
        Assert.Equal(5, option.Nights);
        Assert.Equal(TourDepartureBoardType.Breakfast, option.BoardType);
        Assert.Single(departure.AccommodationOptions);
    }

    [Fact]
    public void AddAccommodationOption_Rejects_Empty_PlaceId()
    {
        Assert.Throws<ArgumentException>(() =>
            NewDeparture().AddAccommodationOption(Guid.Empty, 3, TourDepartureBoardType.None, Now));
    }

    [Fact]
    public void AddAccommodationOption_Rejects_NonPositive_Nights()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NewDeparture().AddAccommodationOption(PlaceId, 0, TourDepartureBoardType.HalfBoard, Now));
    }
}
