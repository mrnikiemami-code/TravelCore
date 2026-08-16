using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoDestinationPublicationHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SeoRedirectHostFixture _fixture;

    public SeoDestinationPublicationHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_CreatesSeoRoute_Idempotent_AndConflictOnOtherResource()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var destA = Guid.Parse("0198a000-0000-7000-8000-000000000901");
        var destB = Guid.Parse("0198a000-0000-7000-8000-000000000902");

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var created = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(createdBody);
        Assert.True(createdBody.Created);
        Assert.Equal("destinations/istanbul", createdBody.PublicPath);
        Assert.Equal("/en/destinations/istanbul", $"/{createdBody.Route.Locale}/{createdBody.Route.Path}".Replace("//", "/"));

        var again = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
        var againBody = await again.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(againBody);
        Assert.False(againBody.Created);
        Assert.False(againBody.PathChanged);

        var conflict = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destB, "en", "istanbul"),
            ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        // Path change for same destination publishes new slug and keeps history via ChangePath.
        var changed = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul-city"),
            ct);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var changedBody = await changed.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(changedBody);
        Assert.True(changedBody.PathChanged);
        Assert.Equal("destinations/istanbul-city", changedBody.PublicPath);

        // IndexPolicy still missing → compose remains noindex (R2; no mass flip on publish).
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var metadata = scope.ServiceProvider.GetRequiredService<ISeoMetadataService>();
            var composed = await metadata.ComposeAsync(
                new ComposeSeoMetadataRequest("en", "destinations/istanbul-city", "Istanbul", null),
                ct);
            Assert.Equal("NoIndex", composed.EffectiveIndex);
            Assert.False(composed.IsIndexable);
        }
    }
}
