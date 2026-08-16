using NodaTime;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoRedirectEngineTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 8, 0);
    private static readonly Guid ResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000101");
    private static readonly Guid OtherResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000102");

    [Fact]
    public void Resolve_ActiveCurrentRoute_WinsOverRedirectSource()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var stale = SeoRedirect.CreatePermanent(
            route.Id,
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/istanbul",
            "destinations/elsewhere",
            Now);

        var resolution = SeoRedirectEngine.Resolve("en", "destinations/istanbul", [route], [stale]);

        Assert.Equal(SeoPathResolutionKind.CurrentRoute, resolution.Kind);
        Assert.Equal(route.Id.Value, resolution.SeoRouteId);
    }

    [Fact]
    public void Resolve_OldPath_YieldsPermanentRedirectToCurrent()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var redirect = SeoRedirect.CreatePermanent(
            route.Id,
            SeoResourceType.Destination,
            ResourceId,
            "en",
            "destinations/istanbul-city",
            "destinations/istanbul",
            Now);

        var resolution = SeoRedirectEngine.Resolve("en", "destinations/istanbul-city", [route], [redirect]);

        Assert.Equal(SeoPathResolutionKind.PermanentRedirect, resolution.Kind);
        Assert.Equal("destinations/istanbul", resolution.TargetPath);
    }

    [Fact]
    public void Resolve_ChainAtoBthenBtoC_FlattensToAtoC()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/c", Now);
        var aToB = SeoRedirect.CreatePermanent(
            route.Id, SeoResourceType.Destination, ResourceId, "en", "destinations/a", "destinations/b", Now);
        var bToC = SeoRedirect.CreatePermanent(
            route.Id, SeoResourceType.Destination, ResourceId, "en", "destinations/b", "destinations/c", Now.Plus(Duration.FromMinutes(1)));

        var fromA = SeoRedirectEngine.Resolve("en", "destinations/a", [route], [aToB, bToC]);
        var fromB = SeoRedirectEngine.Resolve("en", "destinations/b", [route], [aToB, bToC]);

        Assert.Equal("destinations/c", fromA.TargetPath);
        Assert.Equal("destinations/c", fromB.TargetPath);
        Assert.Equal(SeoPathResolutionKind.PermanentRedirect, fromA.Kind);
        Assert.Equal(SeoPathResolutionKind.PermanentRedirect, fromB.Kind);
    }

    [Fact]
    public void ComputePermanentTarget_RejectsSelfRedirect()
    {
        Assert.Throws<SeoRedirectException>(() =>
            SeoRedirectEngine.ComputePermanentTarget(
                "en",
                "destinations/a",
                "destinations/a",
                [],
                []));
    }

    [Fact]
    public void ComputePermanentTarget_RejectsTwoNodeLoop()
    {
        var bToA = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/b", "destinations/a", Now);

        Assert.Throws<SeoRedirectException>(() =>
            SeoRedirectEngine.ComputePermanentTarget(
                "en",
                "destinations/a",
                "destinations/b",
                [],
                [bToA]));
    }

    [Fact]
    public void Resolve_LongerCycle_FailsClosedBounded()
    {
        var a = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/a", "destinations/b", Now);
        var b = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/b", "destinations/c", Now);
        var c = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/c", "destinations/a", Now);

        Assert.Throws<SeoRedirectException>(() =>
            SeoRedirectEngine.Resolve("en", "destinations/a", [], [a, b, c]));
    }

    [Fact]
    public void Resolve_PreservesLocale_DoesNotCrossLocale()
    {
        var enRoute = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var faRedirect = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "fa", "destinations/istanbul-city", "destinations/استانبول", Now);

        var enLookup = SeoRedirectEngine.Resolve("en", "destinations/istanbul-city", [enRoute], [faRedirect]);
        Assert.Equal(SeoPathResolutionKind.NotFound, enLookup.Kind);

        var faLookup = SeoRedirectEngine.Resolve("fa", "destinations/istanbul-city", [enRoute], [faRedirect]);
        Assert.Equal(SeoPathResolutionKind.PermanentRedirect, faLookup.Kind);
        Assert.Equal("fa", faLookup.Locale);
        Assert.Equal("destinations/استانبول", faLookup.TargetPath);
    }

    [Fact]
    public void Resolve_KnownGone_Is410_Unknown_IsNotFound()
    {
        var gone = SeoRedirect.CreateGone(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/retired", Now);

        var known = SeoRedirectEngine.Resolve("en", "destinations/retired", [], [gone]);
        var unknown = SeoRedirectEngine.Resolve("en", "destinations/never-existed", [], [gone]);

        Assert.Equal(SeoPathResolutionKind.Gone, known.Kind);
        Assert.Null(known.TargetPath);
        Assert.Equal(SeoPathResolutionKind.NotFound, unknown.Kind);
    }

    [Fact]
    public void FlattenRedirectGraph_RetargetsAtoB_WhenBMovesToC()
    {
        var aToB = SeoRedirect.CreatePermanent(
            null, SeoResourceType.Destination, ResourceId, "en", "destinations/a", "destinations/b", Now);
        var redirects = new List<SeoRedirect> { aToB };

        SeoRedirectEngine.FlattenRedirectGraph(
            "en",
            "destinations/b",
            "destinations/c",
            redirects,
            Now.Plus(Duration.FromMinutes(2)));

        Assert.Equal("destinations/c", aToB.ToPath);
    }

    [Fact]
    public void SelectCanonical_SelfCanonicalForActiveRoute()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var canonical = SeoRedirectEngine.SelectCanonical("en", "destinations/istanbul", [route], []);

        Assert.NotNull(canonical);
        Assert.True(canonical.IsSelfCanonical);
        Assert.Equal("destinations/istanbul", canonical.Path);
        Assert.Equal(ResourceId, canonical.ResourceId);
        Assert.NotEqual(OtherResourceId, canonical.ResourceId);
    }

    [Fact]
    public void CreatePermanent_RejectsSelfRedirect()
    {
        Assert.Throws<SeoRedirectException>(() =>
            SeoRedirect.CreatePermanent(
                null,
                SeoResourceType.Destination,
                ResourceId,
                "en",
                "destinations/a",
                "destinations/a",
                Now));
    }
}
