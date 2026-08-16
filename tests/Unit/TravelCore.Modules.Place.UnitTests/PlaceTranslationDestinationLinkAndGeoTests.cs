using NodaTime;
using TravelCore.Modules.Place.Domain;
using Xunit;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.UnitTests;

public sealed class PlaceTranslationDestinationLinkAndGeoTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 20, 0);

    [Fact]
    public void SamePlaceId_CanHoldFaAndEnTranslations()
    {
        var place = PlaceAggregate.CreateHotel("HTL-LOC-1", "Grand Hotel", Now, starRating: 4);

        var fa = place.UpsertTranslation("fa", "هتل بزرگ", "توضیح فارسی", Now);
        var en = place.UpsertTranslation("EN", "Grand Hotel", "English description", Now);

        Assert.Equal(place.Id, fa.PlaceId);
        Assert.Equal(place.Id, en.PlaceId);
        Assert.Equal("fa", fa.LocaleCode);
        Assert.Equal("en", en.LocaleCode);
        Assert.Equal(2, place.Translations.Count);

        place.UpsertTranslation("fa", "هتل بزرگ‌تر", null, Now);
        Assert.Equal(2, place.Translations.Count);
        Assert.Equal("هتل بزرگ‌تر", place.FindTranslation("fa")!.Name);
        Assert.Null(place.FindTranslation("fa")!.Description);
        Assert.Equal("Grand Hotel", place.FindTranslation("en")!.Name);
    }

    [Fact]
    public void DestinationLink_AllowsNull_RejectsEmptyGuid()
    {
        var place = PlaceAggregate.CreateHotel("HTL-DEST-1", "Link Hotel", Now, starRating: 3);

        place.SetDestinationLink(null, Now);
        Assert.Null(place.DestinationId);

        var destinationId = Guid.Parse("01900000-0000-7000-8000-0000000000aa");
        place.SetDestinationLink(destinationId, Now);
        Assert.Equal(destinationId, place.DestinationId);

        Assert.Throws<ArgumentException>(() => place.SetDestinationLink(Guid.Empty, Now));

        place.SetDestinationLink(null, Now);
        Assert.Null(place.DestinationId);
    }

    [Fact]
    public void GeographicCoordinates_RequiresPairAndValidRanges()
    {
        var place = PlaceAggregate.CreateAttraction("ATR-GEO-1", "Tower", Now, categoryCode: "landmark");

        place.SetGeographicCoordinates(35.6892m, 51.3890m, Now);
        Assert.Equal(35.689200m, place.Latitude);
        Assert.Equal(51.389000m, place.Longitude);

        Assert.Throws<ArgumentException>(() => place.SetGeographicCoordinates(35.6m, null, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => place.SetGeographicCoordinates(95m, 51m, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => place.SetGeographicCoordinates(35m, 200m, Now));

        place.SetGeographicCoordinates(null, null, Now);
        Assert.Null(place.Latitude);
        Assert.Null(place.Longitude);
    }

    [Fact]
    public void Address_NormalizesAndRejectsInvalidCountryCode()
    {
        var address = PlaceAddress.Create(
            "  Valiasr St  ",
            null,
            " Tehran ",
            "Tehran",
            "12345",
            "ir");

        Assert.NotNull(address);
        Assert.Equal("Valiasr St", address!.Line1);
        Assert.Equal("Tehran", address.Locality);
        Assert.Equal("IR", address.CountryCode);

        Assert.Null(PlaceAddress.Create(null, null, null, null, null, null));
        Assert.Throws<ArgumentException>(() =>
            PlaceAddress.Create("Line", null, null, null, null, "IRN"));
    }

    [Fact]
    public void PlaceTranslation_Slug_NormalizesLikeDestination_AndIsLocaleOwned()
    {
        var place = PlaceAggregate.CreateHotel("htl-slug", "Slug Hotel", Now, starRating: 4);
        place.UpsertTranslation("en", "Slug Hotel", "Desc", Now);
        place.SetTranslationSlug("en", "Grand-Hotel", Now);

        var en = place.FindTranslation("en")!;
        Assert.Equal("grand-hotel", en.Slug);

        Assert.Throws<ArgumentException>(() => place.SetTranslationSlug("en", "Bad_Slug", Now));
        Assert.Throws<ArgumentException>(() => place.SetTranslationSlug("en", "-leading", Now));
    }
}
