using NodaTime;
using TravelCore.Modules.Place.Domain;
using Xunit;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.UnitTests;

public sealed class PlaceFacilitiesClassificationAndCatalogStatusTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 21, 0);

    [Fact]
    public void Create_DefaultsCatalogStatusToDraft()
    {
        var place = PlaceAggregate.CreateHotel("HTL-ST-1", "Status Hotel", Now, starRating: 3);

        Assert.Equal(PlaceCatalogStatus.Draft, place.CatalogStatus);
        Assert.Null(place.ClassificationCode);
        Assert.Empty(place.Facilities);
    }

    [Fact]
    public void SetCatalogStatus_AllowsDraftActiveInactive_RejectsUndefined()
    {
        var place = PlaceAggregate.CreateRestaurant("RST-ST-1", "Status Bistro", Now);

        place.SetCatalogStatus(PlaceCatalogStatus.Active, Now);
        Assert.Equal(PlaceCatalogStatus.Active, place.CatalogStatus);

        place.SetCatalogStatus(PlaceCatalogStatus.Inactive, Now);
        Assert.Equal(PlaceCatalogStatus.Inactive, place.CatalogStatus);

        place.SetCatalogStatus(PlaceCatalogStatus.Draft, Now);
        Assert.Equal(PlaceCatalogStatus.Draft, place.CatalogStatus);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            place.SetCatalogStatus((PlaceCatalogStatus)99, Now));
    }

    [Fact]
    public void SetClassificationCode_NormalizesAndClears()
    {
        var place = PlaceAggregate.CreateAttraction("ATR-CL-1", "Museum", Now, categoryCode: "museum");

        place.SetClassificationCode("  Heritage-Site ", Now);
        Assert.Equal("heritage-site", place.ClassificationCode);

        place.SetClassificationCode(null, Now);
        Assert.Null(place.ClassificationCode);

        Assert.Throws<ArgumentException>(() => place.SetClassificationCode("bad code!", Now));
    }

    [Fact]
    public void ReplaceFacilities_DedupesNormalizesAndClears()
    {
        var place = PlaceAggregate.CreateHotel("HTL-FAC-1", "Facility Hotel", Now, starRating: 4);

        place.ReplaceFacilities([" WiFi ", "PARKING", "wifi", "pool"], Now);
        Assert.Equal(["parking", "pool", "wifi"], place.Facilities.Select(f => f.Code).ToArray());

        place.ReplaceFacilities([], Now);
        Assert.Empty(place.Facilities);

        Assert.Throws<ArgumentException>(() => place.ReplaceFacilities(["bad code!"], Now));
    }
}
