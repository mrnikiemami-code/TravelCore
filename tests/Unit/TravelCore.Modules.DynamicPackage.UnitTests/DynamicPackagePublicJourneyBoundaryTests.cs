using TravelCore.Modules.DynamicPackage.Domain;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackagePublicJourneyBoundaryTests
{
    [Fact]
    public void PackagePublicJourneyBoundary_Allowed_Posture_IsAsExpected()
    {
        Assert.True(PackagePublicJourneyBoundary.DiscoveryConceptAllowed);
        Assert.True(PackagePublicJourneyBoundary.TransientCompositionSelectionAllowed);
        Assert.False(PackagePublicJourneyBoundary.OperationalMutationAllowed);
    }

    [Fact]
    public void PackagePublicJourneyBoundary_TokenStrategy_DoesNotReuse()
    {
        Assert.False(PackagePublicJourneyBoundary.ReuseFlightToken);
        Assert.False(PackagePublicJourneyBoundary.ReuseHotelToken);
    }

    [Fact]
    public void PackagePublicJourneyBoundary_Seo_Posture()
    {
        Assert.True(PackagePublicJourneyBoundary.DiscoveryPagesMayIndex);
        Assert.True(PackagePublicJourneyBoundary.TransactionalPagesNoIndex);
    }

    [Fact]
    public void PackagePublicJourneyBoundary_DistributedTransaction_NotAllowed()
    {
        Assert.False(PackagePublicJourneyBoundary.DistributedTransactionAllowed);
    }
}

