using NodaTime;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoPathCoordinationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 6, 0);
    private static readonly Guid ResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000001");
    private static readonly Guid OtherResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000002");

    [Fact]
    public void ChangePath_TracksHistoryAndRedirectCandidate_WithoutDuplicatingResourceIdentity()
    {
        var route = SeoRoute.Create(
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/istanbul-city",
            Now);

        var later = Now.Plus(Duration.FromMinutes(5));
        var change = route.ChangePath("destinations/istanbul", later);

        Assert.Equal("destinations/istanbul", route.Path);
        Assert.Equal(later, route.UpdatedAt);
        Assert.Equal(ResourceId, route.ResourceId);
        Assert.Equal(ResourceId, change.History.ResourceId);
        Assert.Equal(ResourceId, change.RedirectCandidate.ResourceId);
        Assert.Equal(route.Id, change.History.SeoRouteId);
        Assert.Equal(route.Id, change.RedirectCandidate.SeoRouteId);
        Assert.Equal("destinations/istanbul-city", change.History.Path);
        Assert.Equal("destinations/istanbul", change.History.SucceededByPath);
        Assert.Equal("destinations/istanbul-city", change.RedirectCandidate.FromPath);
        Assert.Equal("destinations/istanbul", change.RedirectCandidate.ToPath);
        Assert.Equal(SeoRedirectCandidateStatus.Pending, change.RedirectCandidate.Status);
    }

    [Fact]
    public void ChangePath_RejectsIdenticalPath()
    {
        var route = SeoRoute.Create(
            SeoResourceType.Destination,
            ResourceId,
            "fa",
            "destinations/استانبول",
            Now);

        Assert.Throws<ArgumentException>(() => route.ChangePath("destinations/استانبول", Now));
    }

    [Fact]
    public void PathHistory_DoesNotModelDestinationTranslationSlugOwnership()
    {
        // Boundary proof: history is keyed by SeoRoute + resource identity + path string,
        // not by Destination.Translation table fields.
        var routeId = SeoRouteId.New();
        var entry = SeoPathHistoryEntry.Record(
            routeId,
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/old",
            "destinations/new",
            Now);

        Assert.Equal(routeId, entry.SeoRouteId);
        Assert.Equal(SeoResourceType.Destination, entry.ResourceType);
        Assert.Equal("destinations/old", entry.Path);
        Assert.DoesNotContain("Translation", entry.GetType().Name, StringComparison.Ordinal);
    }

    [Fact]
    public void Reservation_BlocksForeignResource_AllowsSameResource()
    {
        var existing = SeoPathReservation.Create(
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/istanbul",
            Now);

        SeoPathReservation.EnsureNoForeignReservation(
            [existing],
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/istanbul");

        var ex = Assert.Throws<SeoRouteConflictException>(() =>
            SeoPathReservation.EnsureNoForeignReservation(
                [existing],
                SeoResourceType.Destination,
                OtherResourceId,
                "en",
                "destinations/istanbul"));

        Assert.Contains("reserved by another resource", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
