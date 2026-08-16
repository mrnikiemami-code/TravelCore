using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoIndexabilityHostTests
{
    private readonly SeoRedirectHostFixture _fixture;

    public SeoIndexabilityHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Indexability_MissingPolicy_ReturnsNoIndexFollow()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000401");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/policy-missing"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(
            new Uri("/api/seo/indexability/en/destinations/policy-missing", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoIndexabilityResponse>(cancellationToken: ct);
        Assert.NotNull(body);
        Assert.False(body.IsIndexable);
        Assert.Equal("NoIndex", body.EffectiveIndex);
        Assert.Equal("Follow", body.EffectiveFollow);
        Assert.Equal("noindex, follow", body.RobotsDirective);
        Assert.Contains("missing-policy-default-noindex", body.Reasons);
    }

    [Fact]
    public async Task Indexability_ExplicitIndex_EligibleCurrent_IsIndexable()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000402");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var policies = scope.ServiceProvider.GetRequiredService<ISeoIndexPolicyService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/policy-index"),
                ct);
            await policies.SetAsync(
                new SetSeoIndexPolicyRequest(
                    "Destination",
                    resourceId,
                    "en",
                    "Index",
                    "Follow"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(
            new Uri("/api/seo/indexability/en/destinations/policy-index", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoIndexabilityResponse>(cancellationToken: ct);
        Assert.NotNull(body);
        Assert.True(body.IsIndexable);
        Assert.Equal("index, follow", body.RobotsDirective);
    }

    [Fact]
    public async Task Indexability_RedirectSource_RemainsNoIndex_EvenWithExplicitIndex()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000403");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var policies = scope.ServiceProvider.GetRequiredService<ISeoIndexPolicyService>();
            var created = await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/old-policy"),
                ct);
            await routes.ChangePathAsync(
                created.Id,
                new ChangeSeoRoutePathRequest("destinations/new-policy"),
                ct);
            await policies.SetAsync(
                new SetSeoIndexPolicyRequest("Destination", resourceId, "en", "Index", "Follow"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(
            new Uri("/api/seo/indexability/en/destinations/old-policy", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SeoIndexabilityResponse>(cancellationToken: ct);
        Assert.NotNull(body);
        Assert.False(body.IsIndexable);
        Assert.Contains(body.Reasons, r => r.Contains("historical-redirect", StringComparison.Ordinal));
    }
}
