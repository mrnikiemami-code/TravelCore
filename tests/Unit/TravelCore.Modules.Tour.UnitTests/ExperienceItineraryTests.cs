using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class ExperienceItineraryTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 5, 0);

    private static TourExperienceSpecialization CreateExperienceSpec(string code = "EXP-ITIN-001")
    {
        var product = TourProduct.CreateExperience(code, "Caspian Walk", Now);
        return TourExperienceSpecialization.CreateFor(product, Now);
    }

    [Fact]
    public void EnsureItinerary_CreatesZeroToOneChild()
    {
        var spec = CreateExperienceSpec();

        var first = spec.EnsureItinerary(Now);
        var second = spec.EnsureItinerary(Now.Plus(Duration.FromMinutes(1)));

        Assert.Same(first, second);
        Assert.Equal(spec.TourProductId, first.TourProductId);
        Assert.Empty(first.Days);
    }

    [Fact]
    public void AddDay_OrdersByDayNumber_AndRejectsDuplicates()
    {
        var spec = CreateExperienceSpec("EXP-ITIN-002");
        var itinerary = spec.EnsureItinerary(Now);

        itinerary.AddDay(2, Now);
        itinerary.AddDay(1, Now);

        Assert.Equal(new[] { 1, 2 }, itinerary.DaysOrdered.Select(x => x.DayNumber).ToArray());
        Assert.Throws<ArgumentException>(() => itinerary.AddDay(1, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => itinerary.AddDay(0, Now));
    }

    [Fact]
    public void AddStop_AttachesToDay_WithoutDestinationOrAttractionFields()
    {
        var spec = CreateExperienceSpec("EXP-ITIN-003");
        var itinerary = spec.EnsureItinerary(Now);
        var day = itinerary.AddDay(1, Now);

        var stopA = itinerary.AddStop(day.Id, Now);
        var stopB = itinerary.AddStop(day.Id, Now);

        Assert.Equal(0, stopA.SortOrder);
        Assert.Equal(1, stopB.SortOrder);
        Assert.Equal(new[] { 0, 1 }, day.StopsOrdered.Select(x => x.SortOrder).ToArray());

        var stopType = typeof(ExperienceItineraryStop);
        Assert.Null(stopType.GetProperty("DestinationId"));
        Assert.Null(stopType.GetProperty("PlaceId"));
        Assert.Null(stopType.GetProperty("AttractionId"));
    }

    [Fact]
    public void AddStop_RejectsUnknownDay()
    {
        var spec = CreateExperienceSpec("EXP-ITIN-004");
        var itinerary = spec.EnsureItinerary(Now);

        Assert.Throws<ArgumentException>(() => itinerary.AddStop(ItineraryDayId.New(), Now));
    }

    [Fact]
    public void RemoveDay_RemovesOwnedStops()
    {
        var spec = CreateExperienceSpec("EXP-ITIN-005");
        var itinerary = spec.EnsureItinerary(Now);
        var day = itinerary.AddDay(1, Now);
        itinerary.AddStop(day.Id, Now);

        Assert.True(itinerary.RemoveDay(day.Id, Now));
        Assert.Empty(itinerary.Days);
    }
}
