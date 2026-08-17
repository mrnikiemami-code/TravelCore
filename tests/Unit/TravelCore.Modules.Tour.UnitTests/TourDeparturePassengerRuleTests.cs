using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDeparturePassengerRuleTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 8, 0);

    private static TourDeparture NewDeparture()
    {
        var product = TourProduct.CreatePackage("PKG-PAX-001", "Pax Package", Now);
        return TourDeparture.Create(product, Now);
    }

    [Fact]
    public void SetPassengerRule_Attaches_Acceptance_Policy()
    {
        var departure = NewDeparture();
        departure.SetPassengerRule(1, childAllowed: true, infantAllowed: false, maximumPassengers: 4, Now);

        Assert.NotNull(departure.PassengerRule);
        Assert.Equal(1, departure.PassengerRule!.MinimumAdults);
        Assert.True(departure.PassengerRule.ChildAllowed);
        Assert.False(departure.PassengerRule.InfantAllowed);
        Assert.Equal(4, departure.PassengerRule.MaximumPassengers);
    }

    [Fact]
    public void PassengerRule_Rejects_Max_Less_Than_MinAdults()
    {
        Assert.Throws<ArgumentException>(() =>
            TourDeparturePassengerRule.Create(3, true, true, 2));
    }

    [Fact]
    public void PassengerRule_Rejects_NonPositive_Maximum()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TourDeparturePassengerRule.Create(0, false, false, 0));
    }
}
