using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageConfirmationInvariantsTests
{
    [Fact]
    public void TransientPackageConfirmation_Requires_Candidate()
    {
        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(100m, "IRR"),
            new Money.Money(200m, "IRR"));

        Assert.Throws<ArgumentNullException>(() =>
            TransientPackageConfirmation.ConfirmedPackage(null!, monetary));
    }

    [Fact]
    public void TransientPackageConfirmation_Requires_Monetary()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(),
            HotelBookingId.New());

        Assert.Throws<ArgumentNullException>(() =>
            TransientPackageConfirmation.ConfirmedPackage(candidate, null!));
    }

    [Fact]
    public void TransientPackageConfirmation_Sets_Confirmed_True()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(),
            HotelBookingId.New());

        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(100m, "IRR"),
            new Money.Money(200m, "IRR"));

        var confirmation = TransientPackageConfirmation.ConfirmedPackage(candidate, monetary);

        Assert.True(confirmation.Confirmed);
        Assert.Same(candidate, confirmation.Candidate);
        Assert.Same(monetary, confirmation.Monetary);
    }
}

