using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoMetadataCompositionHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SeoRedirectHostFixture _fixture;

    public SeoMetadataCompositionHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Compose_MissingPolicy_UsesContentTitle_AndNoIndexFollow()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000701");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/meta-compose"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.PostAsJsonAsync(
            new Uri("/api/seo/metadata/compose", UriKind.Relative),
            new ComposeSeoMetadataRequest(
                "en",
                "destinations/meta-compose",
                "Istanbul",
                "Domain description"),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoComposedMetadataResponse>(JsonOptions, ct);
        Assert.NotNull(body);
        Assert.Equal("Istanbul", body.Title);
        Assert.Equal("Domain description", body.Description);
        Assert.False(body.UsedTitleOverride);
        Assert.Equal("NoIndex", body.EffectiveIndex);
        Assert.Equal("Follow", body.EffectiveFollow);
        Assert.Equal("noindex, follow", body.RobotsDirective);
        Assert.False(body.IsIndexable);
        Assert.Equal("/en/destinations/meta-compose", body.CanonicalHref);
    }

    [Fact]
    public async Task Compose_OverrideWins_ForTitleDescription()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000702");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var metadata = scope.ServiceProvider.GetRequiredService<ISeoMetadataService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/meta-override"),
                ct);
            await metadata.SetOverrideAsync(
                new SetSeoMetadataOverrideRequest(
                    "Destination",
                    resourceId,
                    "en",
                    "SEO Title Override",
                    "SEO Description Override"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.PostAsJsonAsync(
            new Uri("/api/seo/metadata/compose", UriKind.Relative),
            new ComposeSeoMetadataRequest(
                "en",
                "destinations/meta-override",
                "Domain Title",
                "Domain Description"),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoComposedMetadataResponse>(JsonOptions, ct);
        Assert.NotNull(body);
        Assert.Equal("SEO Title Override", body.Title);
        Assert.Equal("SEO Description Override", body.Description);
        Assert.True(body.UsedTitleOverride);
        Assert.True(body.UsedDescriptionOverride);
        Assert.Equal("NoIndex", body.EffectiveIndex);
    }
}
