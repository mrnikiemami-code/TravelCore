using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoInternalLinkGraphBoundaryTests
{
    [Fact]
    public void SeoInternalLinkReference_Builds_Outbound_Edge()
    {
        var sourceId = Guid.Parse("0198a000-0000-7000-8000-0000000000ee");
        var targetId = Guid.Parse("0198a000-0000-7000-8000-0000000000ef");
        var edge = SeoInternalLinkReference.Outbound(
            SeoResourceType.Destination,
            sourceId,
            SeoResourceType.Article,
            targetId);

        Assert.Equal(SeoInternalLinkDirection.Outbound, edge.Direction);
        Assert.Equal(sourceId, edge.SourceResourceId);
        Assert.Equal(targetId, edge.TargetResourceId);
    }

    [Fact]
    public void InternalLinkGraphBoundary_Keeps_Editorial_And_Crawl_Out()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.InternalLinkGraphBoundaryImplemented);
        Assert.True(SeoInternalLinkGraphBoundary.SeoOwnsGraphOrchestration);
        Assert.False(SeoInternalLinkGraphBoundary.EditorialLinkSoRImplemented);
        Assert.False(SeoInternalLinkGraphBoundary.ExternalCrawlImplemented);
    }
}
