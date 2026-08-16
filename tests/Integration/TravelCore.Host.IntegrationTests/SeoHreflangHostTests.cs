using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoHreflangHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SeoRedirectHostFixture _fixture;

    public SeoHreflangHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Hreflang_FaEnPair_EmitsBoth_MissingLocaleOmitted()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000601");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/hreflang-en"),
                ct);
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "fa", "destinations/hreflang-fa"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(
            new Uri($"/api/seo/hreflang/Destination/{resourceId:D}", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoHreflangBindingsResponse>(JsonOptions, ct);
        Assert.NotNull(body);
        Assert.Equal(2, body.Alternates.Count);
        Assert.Contains(body.Alternates, a => a.Locale == "en" && a.Href == "/en/destinations/hreflang-en");
        Assert.Contains(body.Alternates, a => a.Locale == "fa" && a.Href == "/fa/destinations/hreflang-fa");

        // Single-locale resource omits the other locale (no fabrication).
        var onlyEnId = Guid.Parse("0198a000-0000-7000-8000-000000000602");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", onlyEnId, "en", "destinations/hreflang-only-en"),
                ct);
        }

        var onlyEn = await client.GetAsync(
            new Uri($"/api/seo/hreflang/Destination/{onlyEnId:D}", UriKind.Relative),
            ct);
        var onlyBody = await onlyEn.Content.ReadFromJsonAsync<SeoHreflangBindingsResponse>(JsonOptions, ct);
        Assert.NotNull(onlyBody);
        Assert.Single(onlyBody.Alternates);
        Assert.Equal("en", onlyBody.Alternates[0].Locale);
    }

    [Fact]
    public async Task Hreflang_ByPath_UsesCurrentRoute_NotHistorical()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000603");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var created = await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/old-href"),
                ct);
            await routes.ChangePathAsync(
                created.Id,
                new ChangeSeoRoutePathRequest("destinations/new-href"),
                ct);
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "fa", "destinations/fa-href"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var current = await client.GetAsync(
            new Uri("/api/seo/hreflang/by-path/en/destinations/new-href", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);

        var historical = await client.GetAsync(
            new Uri("/api/seo/hreflang/by-path/en/destinations/old-href", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.NotFound, historical.StatusCode);
    }
}
