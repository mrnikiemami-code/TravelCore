using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureTransportSegmentTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 6, 0);

    private static TourDeparture NewDeparture()
    {
        var product = TourProduct.CreatePackage("PKG-TR-001", "Transport Package", Now);
        return TourDeparture.Create(product, Now);
    }

    [Fact]
    public void AddTransportSegment_Stores_Descriptive_Air_Leg()
    {
        var departure = NewDeparture();
        var segment = departure.AddTransportSegment(
            1,
            TourDepartureTransportMode.Air,
            "Tehran",
            "Istanbul",
            Now);

        Assert.Equal(1, segment.Sequence);
        Assert.Equal(TourDepartureTransportMode.Air, segment.TransportMode);
        Assert.Equal("Tehran", segment.Origin);
        Assert.Equal("Istanbul", segment.Destination);
        Assert.Single(departure.TransportSegments);
    }

    [Fact]
    public void AddTransportSegment_Rejects_Duplicate_Sequence()
    {
        var departure = NewDeparture();
        departure.AddTransportSegment(1, TourDepartureTransportMode.Air, "Tehran", "Istanbul", Now);
        Assert.Throws<InvalidOperationException>(() =>
            departure.AddTransportSegment(1, TourDepartureTransportMode.Ground, "Istanbul", "Antalya", Now));
    }

    [Fact]
    public void AddTransportSegment_Rejects_Blank_Origin()
    {
        var departure = NewDeparture();
        Assert.Throws<ArgumentException>(() =>
            departure.AddTransportSegment(1, TourDepartureTransportMode.Air, " ", "Istanbul", Now));
    }
}
