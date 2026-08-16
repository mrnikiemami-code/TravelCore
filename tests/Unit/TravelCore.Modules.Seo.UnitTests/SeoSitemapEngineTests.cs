using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoSitemapEngineTests
{
    [Fact]
    public void SelectIndexableUrls_ExcludesMissingPolicyAndIncludesEligible()
    {
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000801");
        var candidates = new[]
        {
            new SeoSitemapCandidate("en", "destinations/a", SeoResourceType.Destination, resourceId),
            new SeoSitemapCandidate("en", "destinations/b", SeoResourceType.Destination, resourceId),
        };

        var urls = SeoSitemapEngine.SelectIndexableUrls(
            candidates,
            c => c.Path.EndsWith("/b", StringComparison.Ordinal)
                ? SeoIndexabilityEvaluation.Indexable(
                    c.Locale,
                    c.Path,
                    SeoFollowDirective.Follow,
                    SeoIndexDirective.Index,
                    SeoFollowDirective.Follow,
                    "ok")
                : SeoIndexabilityEvaluation.ConservativeNoIndex(
                    c.Locale,
                    c.Path,
                    null,
                    null,
                    "missing-policy-default-noindex"));

        Assert.Single(urls);
        Assert.Equal("/en/destinations/b", urls[0].Loc);
    }

    [Fact]
    public void RenderUrlSetXml_EscapesAndWraps()
    {
        var xml = SeoSitemapEngine.RenderUrlSetXml(
            [new SeoSitemapUrl("en", "destinations/a", "/en/destinations/a")]);

        Assert.Contains("<urlset", xml, StringComparison.Ordinal);
        Assert.Contains("<loc>/en/destinations/a</loc>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void RobotsTxt_ReferencesSitemapEndpoint()
    {
        var robots = SeoRobotsTxtEngine.Render();
        Assert.Contains("User-agent: *", robots, StringComparison.Ordinal);
        Assert.Contains("Sitemap: /api/seo/sitemap.xml", robots, StringComparison.Ordinal);
    }
}
