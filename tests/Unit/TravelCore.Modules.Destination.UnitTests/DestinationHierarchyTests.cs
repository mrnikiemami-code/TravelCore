using NodaTime;
using TravelCore.Modules.Destination.Domain;
using Xunit;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.UnitTests;

public sealed class DestinationHierarchyTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 15, 22, 0);

    [Fact]
    public void Country_RequiresIsoAndForbidsParent()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "ir");

        Assert.Equal(DestinationKind.Country, country.Kind);
        Assert.Equal("IR", country.IsoCountryCode);
        Assert.Null(country.ParentId);

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.Country,
                "IR2",
                "Iran",
                Now,
                parentId: DestinationId.New(),
                isoCountryCode: "IR"));

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.Country,
                "IR3",
                "Iran",
                Now));
    }

    [Fact]
    public void Region_RequiresCountryParent()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "IR");

        var region = DestinationAggregate.Create(
            DestinationKind.Region,
            "IR-THR",
            "Tehran Province",
            Now,
            parentId: country.Id,
            parent: country);

        Assert.Equal(country.Id, region.ParentId);

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.Region,
                "BAD",
                "Bad",
                Now));

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.Region,
                "BAD2",
                "Bad",
                Now,
                parentId: country.Id,
                parent: region));
    }

    [Fact]
    public void City_AllowsCountryOrRegionParent_AreaAllowsRegionOrCity()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "IR");
        var region = DestinationAggregate.Create(
            DestinationKind.Region,
            "IR-THR",
            "Tehran Province",
            Now,
            parentId: country.Id,
            parent: country);
        var city = DestinationAggregate.Create(
            DestinationKind.City,
            "IR-THR-TEH",
            "Tehran",
            Now,
            parentId: region.Id,
            parent: region);
        var area = DestinationAggregate.Create(
            DestinationKind.Area,
            "IR-THR-TEH-VAL",
            "Valiasr",
            Now,
            parentId: city.Id,
            parent: city);

        Assert.Equal(DestinationKind.Area, area.Kind);

        var cityUnderCountry = DestinationAggregate.Create(
            DestinationKind.City,
            "IR-CITY",
            "Direct City",
            Now,
            parentId: country.Id,
            parent: country);
        Assert.Equal(country.Id, cityUnderCountry.ParentId);

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.Area,
                "BAD-AREA",
                "Bad",
                Now,
                parentId: country.Id,
                parent: country));

        Assert.Throws<ArgumentException>(() =>
            DestinationAggregate.Create(
                DestinationKind.City,
                "BAD-CITY",
                "Bad",
                Now,
                parentId: city.Id,
                parent: city,
                isoCountryCode: "IR"));
    }

    [Fact]
    public void ClosedKind_DoesNotAcceptPlaceOrHotel()
    {
        Assert.False(Enum.TryParse<DestinationKind>("Place", ignoreCase: true, out var place) && Enum.IsDefined(place));
        Assert.False(Enum.IsDefined(typeof(DestinationKind), (short)99));
        Assert.Equal(4, Enum.GetValues<DestinationKind>().Length);
    }
}
