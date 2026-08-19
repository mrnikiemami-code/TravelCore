using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageOrchestrationInvariantsTests
{
    [Fact]
    public void PackageOrchestrationPlan_Create_Succeeds()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(), HotelBookingId.New());
        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(1000m, "IRR"), new Money.Money(2000m, "IRR"));

        var plan = PackageOrchestrationPlan.Create(candidate, monetary);

        Assert.Same(candidate, plan.Candidate);
        Assert.Same(monetary, plan.Monetary);
    }

    [Fact]
    public void PackageOrchestrationPlan_Create_NullCandidate_Rejected()
    {
        var monetary = PackageMonetarySnapshot.Create(
            new Money.Money(1000m, "IRR"), new Money.Money(2000m, "IRR"));

        Assert.Throws<ArgumentNullException>(() =>
            PackageOrchestrationPlan.Create(null!, monetary));
    }

    [Fact]
    public void PackageOrchestrationPlan_Create_NullMonetary_Rejected()
    {
        var candidate = TransientPackageCandidate.Create(
            FlightBookingId.New(), HotelBookingId.New());

        Assert.Throws<ArgumentNullException>(() =>
            PackageOrchestrationPlan.Create(candidate, null!));
    }

    [Fact]
    public void PackageOrchestrationPlan_DistributedTransaction_NotAllowed()
    {
        Assert.False(PackageOrchestrationPlan.DistributedTransactionAllowed);
    }

    [Fact]
    public void PackageOrchestrationPlan_Saga_NotImplemented()
    {
        Assert.False(PackageOrchestrationPlan.SagaImplemented);
    }

    [Fact]
    public void PackageOrchestrationPlan_Compensation_NotImplemented()
    {
        Assert.False(PackageOrchestrationPlan.CompensationImplemented);
    }

    [Fact]
    public void PackageOrchestrationPlan_CoordinationPattern_IsChoreography()
    {
        Assert.Contains("Choreography", PackageOrchestrationPlan.CoordinationPattern);
    }
}
