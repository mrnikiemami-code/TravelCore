using NodaTime;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoRouteTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 5, 30);

    [Fact]
    public void Create_BindsResourceWithoutOwningDestinationContent()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
        var route = SeoRoute.Create(
            SeoResourceType.Destination,
            resourceId,
            "FA",
            "/destinations/istanbul/",
            Now);

        Assert.Equal(SeoResourceType.Destination, route.ResourceType);
        Assert.Equal(resourceId, route.ResourceId);
        Assert.Equal("fa", route.Locale);
        Assert.Equal("destinations/istanbul", route.Path);
        Assert.Equal(Now, route.CreatedAt);
        Assert.Equal(Now, route.UpdatedAt);
    }

    [Fact]
    public void NormalizePath_RejectsEmptyAndTraversalSegments()
    {
        Assert.Throws<ArgumentException>(() => SeoRoute.NormalizePath("   "));
        Assert.Throws<ArgumentException>(() => SeoRoute.NormalizePath("/"));
        Assert.Throws<ArgumentException>(() => SeoRoute.NormalizePath("destinations//istanbul"));
        Assert.Throws<ArgumentException>(() => SeoRoute.NormalizePath("destinations/../istanbul"));
        Assert.Throws<ArgumentException>(() => SeoRoute.NormalizePath("destinations/bad slug"));
    }

    [Fact]
    public void EnsureNoConflict_RejectsSameLocalePathForDifferentResource()
    {
        var existingId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
        var otherId = Guid.Parse("0198a000-0000-7000-8000-000000000002");
        var existing = SeoRoute.Create(
            SeoResourceType.Destination,
            existingId,
            "en",
            "destinations/istanbul",
            Now);

        var ex = Assert.Throws<SeoRouteConflictException>(() =>
            SeoRoute.EnsureNoConflict(
                [existing],
                SeoResourceType.Destination,
                otherId,
                "en",
                "destinations/istanbul"));

        Assert.Contains("already bound", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureNoConflict_RejectsSecondPathForSameResourceLocale()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
        var existing = SeoRoute.Create(
            SeoResourceType.Destination,
            resourceId,
            "fa",
            "destinations/istanbul",
            Now);

        var ex = Assert.Throws<SeoRouteConflictException>(() =>
            SeoRoute.EnsureNoConflict(
                [existing],
                SeoResourceType.Destination,
                resourceId,
                "fa",
                "destinations/istanbull"));

        Assert.Contains("already has an active path", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureNoConflict_AllowsSameResourceSamePathIdempotentShape()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
        var existing = SeoRoute.Create(
            SeoResourceType.Destination,
            resourceId,
            "en",
            "destinations/istanbul",
            Now);

        // Identical binding is not a cross-resource conflict; create path uniqueness is DB-owned.
        SeoRoute.EnsureNoConflict(
            [existing],
            SeoResourceType.Destination,
            resourceId,
            "en",
            "destinations/istanbul");
    }

    [Fact]
    public void EnsureNoConflict_AllowsSamePathAcrossDifferentLocales()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
        var existing = SeoRoute.Create(
            SeoResourceType.Destination,
            resourceId,
            "fa",
            "destinations/istanbul",
            Now);

        SeoRoute.EnsureNoConflict(
            [existing],
            SeoResourceType.Destination,
            resourceId,
            "en",
            "destinations/istanbul");
    }
}
