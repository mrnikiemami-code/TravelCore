using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureCapacityTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 4, 0);

    [Fact]
    public void SetCapacity_Attaches_Min_And_Max_Pax()
    {
        var product = TourProduct.CreatePackage("PKG-CAP-001", "Europe Package", Now);
        var departure = TourDeparture.Create(product, Now);

        departure.SetCapacity(4, 20, Now);

        Assert.NotNull(departure.Capacity);
        Assert.Equal(4, departure.Capacity!.MinimumPax);
        Assert.Equal(20, departure.Capacity.MaximumPax);
    }

    [Fact]
    public void Capacity_Allows_Zero_Minimum()
    {
        var capacity = TourDepartureCapacity.Create(0, 10);
        Assert.Equal(0, capacity.MinimumPax);
        Assert.Equal(10, capacity.MaximumPax);
    }

    [Fact]
    public void Capacity_Allows_Equal_Min_And_Max()
    {
        var capacity = TourDepartureCapacity.Create(8, 8);
        Assert.Equal(8, capacity.MinimumPax);
        Assert.Equal(8, capacity.MaximumPax);
    }

    [Fact]
    public void Capacity_Rejects_Negative_Minimum()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TourDepartureCapacity.Create(-1, 10));
    }

    [Fact]
    public void Capacity_Rejects_NonPositive_Maximum()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TourDepartureCapacity.Create(0, 0));
    }

    [Fact]
    public void Capacity_Rejects_Max_Less_Than_Min()
    {
        Assert.Throws<ArgumentException>(() => TourDepartureCapacity.Create(10, 5));
    }
}
