using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using TravelCore.Modules.Seo.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoContentGraphFoundationTests
{
    [Fact]
    public void SeoContentGraphReference_Normalizes_And_Validates()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-0000000000aa");
        var reference = SeoContentGraphReference.From("Destination", resourceId);
        Assert.Equal("Destination", reference.ResourceType);
        Assert.Equal(resourceId, reference.ResourceId);

        Assert.Throws<ArgumentException>(() => SeoContentGraphReference.From(" ", resourceId));
        Assert.Throws<ArgumentException>(() => SeoContentGraphReference.From("Destination", Guid.Empty));
    }

    [Fact]
    public void SeoContentGraphNode_Registers_Publishable_Resource()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-0000000000bb");
        var now = Instant.FromUtc(2026, 8, 19, 18, 0);
        var node = SeoContentGraphNode.Register(SeoResourceType.Article, resourceId, now);

        Assert.NotEqual(Guid.Empty, node.Id.Value);
        Assert.Equal(SeoResourceType.Article, node.ResourceType);
        Assert.Equal(resourceId, node.ResourceId);
        Assert.Equal(now, node.CreatedAt);
    }

    [Fact]
    public void SeoContentGraphNode_Rejects_Empty_ResourceId()
    {
        Assert.Throws<ArgumentException>(() =>
            SeoContentGraphNode.Register(SeoResourceType.Place, Guid.Empty, Instant.FromUtc(2026, 8, 19, 18, 0)));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T004_Foundation_Only()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.ContentGraphFoundationImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.HubClusterBoundaryImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.InternalLinkGraphBoundaryImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.ProgrammaticLandingBoundaryImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.PublicGraphMutationApiImplemented);
        Assert.Equal("Seo", SeoContentGraphOwnershipBoundary.OwnerModule);
        Assert.Equal("seo", SeoDbContext.SchemaName);
    }
}
