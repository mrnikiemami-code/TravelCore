using NodaTime;
using TravelCore.Modules.Place.Domain;
using Xunit;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.UnitTests;

public sealed class PlaceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 18, 0);

    [Fact]
    public void PlaceId_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => PlaceId.From(Guid.Empty));
    }

    [Fact]
    public void PlaceId_New_IsNonEmpty()
    {
        var id = PlaceId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void CreateHotel_SetsKindAndSpecialization()
    {
        var place = PlaceAggregate.CreateHotel("HTL-001", " Grand Hotel ", Now, starRating: 4);

        Assert.Equal(PlaceKind.Hotel, place.Kind);
        Assert.Equal("HTL-001", place.Code);
        Assert.Equal("Grand Hotel", place.EnglishName);
        Assert.NotNull(place.Hotel);
        Assert.Equal((short?)4, place.Hotel.StarRating);
        Assert.Null(place.Restaurant);
        Assert.Null(place.Attraction);
        Assert.Equal(place.Id, place.Hotel.PlaceId);
        Assert.NotEqual(Guid.Empty, place.Id.Value);
    }

    [Fact]
    public void CreateRestaurant_SetsKindAndSpecialization()
    {
        var place = PlaceAggregate.CreateRestaurant("RST-001", "Bistro", Now, cuisineType: " Persian ");

        Assert.Equal(PlaceKind.Restaurant, place.Kind);
        Assert.NotNull(place.Restaurant);
        Assert.Equal("Persian", place.Restaurant.CuisineType);
        Assert.Null(place.Hotel);
        Assert.Null(place.Attraction);
    }

    [Fact]
    public void CreateAttraction_SetsKindAndSpecialization()
    {
        var place = PlaceAggregate.CreateAttraction("ATR-001", "Museum", Now, categoryCode: "museum");

        Assert.Equal(PlaceKind.Attraction, place.Kind);
        Assert.NotNull(place.Attraction);
        Assert.Equal("museum", place.Attraction.CategoryCode);
        Assert.Null(place.Hotel);
        Assert.Null(place.Restaurant);
    }

    [Fact]
    public void CreateHotel_RejectsInvalidStarRating()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlaceAggregate.CreateHotel("HTL-X", "X", Now, starRating: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlaceAggregate.CreateHotel("HTL-X", "X", Now, starRating: 6));
    }

    [Fact]
    public void ValidateSpecializationInvariant_RejectsMultiKind()
    {
        var id = PlaceId.New();
        var hotel = Hotel.Create(id, 3);
        var restaurant = Restaurant.Create(id, "italian");

        var ex = Assert.Throws<ArgumentException>(() =>
            PlaceAggregate.ValidateSpecializationInvariant(
                id,
                PlaceKind.Hotel,
                hotel,
                restaurant,
                attraction: null));

        Assert.Contains("only one typed specialization", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateSpecializationInvariant_RejectsKindMismatch_HotelWithoutHotelRow()
    {
        var id = PlaceId.New();
        var restaurant = Restaurant.Create(id, "italian");

        Assert.Throws<ArgumentException>(() =>
            PlaceAggregate.ValidateSpecializationInvariant(
                id,
                PlaceKind.Hotel,
                hotel: null,
                restaurant,
                attraction: null));
    }

    [Fact]
    public void ValidateSpecializationInvariant_RejectsKindMismatch_RestaurantKindWithHotel()
    {
        var id = PlaceId.New();
        var hotel = Hotel.Create(id, 5);

        Assert.Throws<ArgumentException>(() =>
            PlaceAggregate.ValidateSpecializationInvariant(
                id,
                PlaceKind.Restaurant,
                hotel,
                restaurant: null,
                attraction: null));
    }

    [Fact]
    public void Reconstitute_RejectsSpecializationPlaceIdMismatch()
    {
        var placeId = PlaceId.New();
        var otherId = PlaceId.New();
        var hotel = Hotel.Create(otherId, 2);

        Assert.Throws<ArgumentException>(() =>
            PlaceAggregate.Reconstitute(
                placeId,
                PlaceKind.Hotel,
                "HTL-Y",
                "Y",
                Now,
                Now,
                hotel,
                restaurant: null,
                attraction: null));
    }

    [Fact]
    public void Reconstitute_RejectsMissingSpecialization()
    {
        var id = PlaceId.New();

        Assert.Throws<ArgumentException>(() =>
            PlaceAggregate.Reconstitute(
                id,
                PlaceKind.Attraction,
                "ATR-Z",
                "Z",
                Now,
                Now,
                hotel: null,
                restaurant: null,
                attraction: null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankCode(string code)
    {
        Assert.ThrowsAny<ArgumentException>(() => PlaceAggregate.CreateHotel(code, "Name", Now));
    }
}
