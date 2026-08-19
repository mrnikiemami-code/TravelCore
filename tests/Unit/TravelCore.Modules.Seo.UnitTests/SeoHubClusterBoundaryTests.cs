using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoHubClusterBoundaryTests
{
    [Fact]
    public void SeoHubClusterReference_Builds_DestinationHub_And_ContentCluster()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-0000000000dd");
        var hub = SeoHubClusterReference.DestinationHub(SeoResourceType.Destination, resourceId);
        var cluster = SeoHubClusterReference.ContentCluster(SeoResourceType.Article, resourceId);

        Assert.Equal(SeoHubClusterKind.DestinationHub, hub.Kind);
        Assert.Equal(SeoResourceType.Destination, hub.ResourceType);
        Assert.Equal(SeoHubClusterKind.ContentCluster, cluster.Kind);
        Assert.Equal(SeoResourceType.Article, cluster.ResourceType);
    }

    [Fact]
    public void OwnershipBoundary_Records_T005_HubCluster_Posture()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.HubClusterBoundaryImplemented);
        Assert.False(SeoHubClusterBoundary.HubClusterPersistenceImplemented);
        Assert.False(SeoHubClusterBoundary.PublicApiImplemented);
    }
}
