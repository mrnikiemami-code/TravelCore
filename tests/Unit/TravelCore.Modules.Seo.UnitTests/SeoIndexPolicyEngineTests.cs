using NodaTime;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoIndexPolicyEngineTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 9, 0);
    private static readonly Guid ResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000301");

    [Fact]
    public void MissingPolicy_DefaultsToNoIndexFollow()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var resolution = SeoPathResolution.Current("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value);
        var canonical = new SeoCanonicalSelection("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value, true);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/istanbul", null, resolution, canonical);

        Assert.False(result.IsIndexable);
        Assert.Equal(SeoIndexDirective.NoIndex, result.EffectiveIndex);
        Assert.Equal(SeoFollowDirective.Follow, result.EffectiveFollow);
        Assert.Equal("noindex, follow", result.RobotsDirective);
        Assert.Contains("missing-policy-default-noindex", result.Reasons);
    }

    [Fact]
    public void ExplicitNoIndex_RemainsNoIndex()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var policy = SeoIndexPolicy.Create(
            SeoResourceType.Destination, ResourceId, "en", SeoIndexDirective.NoIndex, SeoFollowDirective.Follow, Now);
        var resolution = SeoPathResolution.Current("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value);
        var canonical = new SeoCanonicalSelection("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value, true);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/istanbul", policy, resolution, canonical);

        Assert.False(result.IsIndexable);
        Assert.Contains("explicit-noindex", result.Reasons);
    }

    [Fact]
    public void ExplicitIndex_EligibleCurrentRoute_IsIndexable()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var policy = SeoIndexPolicy.Create(
            SeoResourceType.Destination, ResourceId, "en", SeoIndexDirective.Index, SeoFollowDirective.Follow, Now);
        var resolution = SeoPathResolution.Current("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value);
        var canonical = new SeoCanonicalSelection("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value, true);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/istanbul", policy, resolution, canonical);

        Assert.True(result.IsIndexable);
        Assert.Equal("index, follow", result.RobotsDirective);
        Assert.Contains("explicit-index-and-eligible-current-route", result.Reasons);
    }

    [Fact]
    public void ExplicitIndex_HistoricalRedirect_NotIndexable()
    {
        var policy = SeoIndexPolicy.Create(
            SeoResourceType.Destination, ResourceId, "en", SeoIndexDirective.Index, SeoFollowDirective.Follow, Now);
        var resolution = SeoPathResolution.Permanent(
            "en", "destinations/old", "destinations/istanbul", SeoResourceType.Destination, ResourceId, null);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/old", policy, resolution, canonical: null);

        Assert.False(result.IsIndexable);
        Assert.Contains("explicit-index-but-historical-redirect-source", result.Reasons);
    }

    [Fact]
    public void ExplicitIndex_Gone_NotIndexable()
    {
        var policy = SeoIndexPolicy.Create(
            SeoResourceType.Destination, ResourceId, "en", SeoIndexDirective.Index, SeoFollowDirective.Follow, Now);
        var resolution = SeoPathResolution.GonePath(
            "en", "destinations/retired", SeoResourceType.Destination, ResourceId, null);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/retired", policy, resolution, null);

        Assert.False(result.IsIndexable);
        Assert.Contains("explicit-index-but-gone", result.Reasons);
    }

    [Fact]
    public void ExplicitIndex_MissingCanonical_NotIndexable()
    {
        var route = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var policy = SeoIndexPolicy.Create(
            SeoResourceType.Destination, ResourceId, "en", SeoIndexDirective.Index, SeoFollowDirective.Follow, Now);
        var resolution = SeoPathResolution.Current("en", "destinations/istanbul", SeoResourceType.Destination, ResourceId, route.Id.Value);

        var result = SeoIndexPolicyEngine.Evaluate("en", "destinations/istanbul", policy, resolution, canonical: null);

        Assert.False(result.IsIndexable);
        Assert.Contains("explicit-index-but-canonical-ineligible", result.Reasons);
    }
}
