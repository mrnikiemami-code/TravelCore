using NodaTime;
using TravelCore.Modules.Destination.Domain;
using Xunit;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.UnitTests;

public sealed class DestinationTranslationAndGeoTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 15, 23, 30);

    [Fact]
    public void SameDestinationId_CanHoldFaAndEnTranslations()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "IR");

        var fa = country.UpsertTranslation("fa", "ایران", "کشور ایران", Now);
        var en = country.UpsertTranslation("EN", "Iran", "Country of Iran", Now);

        Assert.Equal(country.Id, fa.DestinationId);
        Assert.Equal(country.Id, en.DestinationId);
        Assert.Equal("fa", fa.LocaleCode);
        Assert.Equal("en", en.LocaleCode);
        Assert.Equal(2, country.Translations.Count);

        country.UpsertTranslation("fa", "ایران عزیز", null, Now);
        Assert.Equal(2, country.Translations.Count);
        Assert.Equal("ایران عزیز", country.FindTranslation("fa")!.Name);
        Assert.Null(country.FindTranslation("fa")!.Description);
    }

    [Fact]
    public void GeographicIdentity_RequiresPairAndValidRanges()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "IR");

        country.SetGeographicIdentity(35.6892m, 51.3890m, Now);
        Assert.Equal(35.689200m, country.Latitude);
        Assert.Equal(51.389000m, country.Longitude);

        Assert.Throws<ArgumentException>(() => country.SetGeographicIdentity(35.6m, null, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => country.SetGeographicIdentity(95m, 51m, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => country.SetGeographicIdentity(35m, 200m, Now));

        country.SetGeographicIdentity(null, null, Now);
        Assert.Null(country.Latitude);
        Assert.Null(country.Longitude);
    }
}
