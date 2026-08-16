using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoSitemapHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SeoRedirectHostFixture _fixture;

    public SeoSitemapHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Sitemap_ExcludesMissingPolicy_IncludesExplicitIndexEligible()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var missingId = Guid.Parse("0198a000-0000-7000-8000-000000000811");
        var indexId = Guid.Parse("0198a000-0000-7000-8000-000000000812");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var policies = scope.ServiceProvider.GetRequiredService<ISeoIndexPolicyService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", missingId, "en", "destinations/sitemap-missing"),
                ct);
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", indexId, "en", "destinations/sitemap-index"),
                ct);
            await policies.SetAsync(
                new SetSeoIndexPolicyRequest(
                    "Destination",
                    indexId,
                    "en",
                    "Index",
                    "Follow"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var json = await client.GetAsync(new Uri("/api/seo/sitemap", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, json.StatusCode);
        var body = await json.Content.ReadFromJsonAsync<SeoSitemapDocumentResponse>(JsonOptions, ct);
        Assert.NotNull(body);
        Assert.DoesNotContain(body.Urls, u => u.Path == "destinations/sitemap-missing");
        Assert.Contains(body.Urls, u => u.Loc == "/en/destinations/sitemap-index");

        var xml = await client.GetAsync(new Uri("/api/seo/sitemap.xml", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, xml.StatusCode);
        var xmlText = await xml.Content.ReadAsStringAsync(ct);
        Assert.Contains("/en/destinations/sitemap-index", xmlText, StringComparison.Ordinal);
        Assert.DoesNotContain("sitemap-missing", xmlText, StringComparison.Ordinal);

        var robots = await client.GetAsync(new Uri("/api/seo/robots.txt", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, robots.StatusCode);
        var robotsText = await robots.Content.ReadAsStringAsync(ct);
        Assert.Contains("Sitemap: /api/seo/sitemap.xml", robotsText, StringComparison.Ordinal);
    }
}
