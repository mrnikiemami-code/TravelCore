using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageMonetaryInvariantsTests
{
    [Fact]
    public void PackageMonetarySnapshot_Create_SameCurrency_Succeeds()
    {
        var flight = new Money.Money(1_500_000m, "IRR");
        var hotel = new Money.Money(2_000_000m, "IRR");

        var snapshot = PackageMonetarySnapshot.Create(flight, hotel);

        Assert.Equal(1_500_000m, snapshot.FlightTotal.Amount);
        Assert.Equal(2_000_000m, snapshot.HotelTotal.Amount);
        Assert.Equal(3_500_000m, snapshot.PackageTotal.Amount);
        Assert.Equal("IRR", snapshot.PackageTotal.Currency.ToString());
    }

    [Fact]
    public void PackageMonetarySnapshot_Create_MixedCurrency_Rejected()
    {
        var flight = new Money.Money(100m, "USD");
        var hotel = new Money.Money(200m, "EUR");

        Assert.Throws<InvalidOperationException>(() =>
            PackageMonetarySnapshot.Create(flight, hotel));
    }

    [Fact]
    public void PackageMonetarySnapshot_Create_NullFlight_Rejected()
    {
        var hotel = new Money.Money(200m, "IRR");

        Assert.Throws<ArgumentNullException>(() =>
            PackageMonetarySnapshot.Create(null!, hotel));
    }

    [Fact]
    public void PackageMonetarySnapshot_Create_NullHotel_Rejected()
    {
        var flight = new Money.Money(100m, "IRR");

        Assert.Throws<ArgumentNullException>(() =>
            PackageMonetarySnapshot.Create(flight, null!));
    }

    [Fact]
    public void TransientPackageQuote_Create_Succeeds()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(), HotelBookingId.New());
        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(1000m, "IRR"), new Money.Money(2000m, "IRR"));

        var quote = TransientPackageQuote.Create(candidate, monetary);

        Assert.Same(candidate, quote.Candidate);
        Assert.Same(monetary, quote.Monetary);
    }

    [Fact]
    public void TransientPackageQuote_Create_NullCandidate_Rejected()
    {
        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(1000m, "IRR"), new Money.Money(2000m, "IRR"));

        Assert.Throws<ArgumentNullException>(() =>
            TransientPackageQuote.Create(null!, monetary));
    }

    [Fact]
    public void TransientPackageQuote_Create_NullMonetary_Rejected()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(), HotelBookingId.New());

        Assert.Throws<ArgumentNullException>(() =>
            TransientPackageQuote.Create(candidate, null!));
    }
}
