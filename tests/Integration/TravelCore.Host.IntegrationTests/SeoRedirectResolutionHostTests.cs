using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

/// <summary>
/// Host HTTP proofs for SEO redirect/canonical resolution (TC-P05-T004).
/// </summary>
public sealed class SeoRedirectHostFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_seo_redirect_host")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<SeoDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: SeoDbContext.SchemaName)
            .Options;
        await using var db = new SeoDbContext(options);
        await SeoMigrator.MigrateAsync(db);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public TravelCoreApiFactory CreateFactory() =>
        new(Environments.Development, ConnectionString);
}

[CollectionDefinition(nameof(SeoRedirectHostCollection), DisableParallelization = true)]
public sealed class SeoRedirectHostCollection : ICollectionFixture<SeoRedirectHostFixture>;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoRedirectResolutionHostTests
{
    private readonly SeoRedirectHostFixture _fixture;

    public SeoRedirectResolutionHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Resolve_OldPath_Returns301_ToCurrentTarget()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000201");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var created = await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/old-city"),
                ct);
            await routes.ChangePathAsync(
                created.Id,
                new ChangeSeoRoutePathRequest("destinations/new-city"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/old-city", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("/en/destinations/new-city", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Resolve_Chain_Flattens_ToFinalTarget()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000202");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var created = await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/a"),
                ct);
            await routes.ChangePathAsync(created.Id, new ChangeSeoRoutePathRequest("destinations/b"), ct);
            await routes.ChangePathAsync(created.Id, new ChangeSeoRoutePathRequest("destinations/c"), ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var fromA = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/a", UriKind.Relative),
            ct);
        var fromB = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/b", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.MovedPermanently, fromA.StatusCode);
        Assert.Equal(HttpStatusCode.MovedPermanently, fromB.StatusCode);
        Assert.Equal("/en/destinations/c", fromA.Headers.Location?.ToString());
        Assert.Equal("/en/destinations/c", fromB.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Resolve_Gone_Returns410_Unknown_Returns404()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000203");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var redirects = scope.ServiceProvider.GetRequiredService<ISeoRedirectService>();
            await redirects.MarkGoneAsync(
                new MarkSeoPathGoneRequest("Destination", resourceId, "en", "destinations/retired", null),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var gone = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/retired", UriKind.Relative),
            ct);
        var missing = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/never-heard-of", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.Gone, gone.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Resolve_CurrentRoute_Returns200_AndCanonicalSelf()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000204");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "en", "destinations/live"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var current = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/live", UriKind.Relative),
            ct);
        var canonical = await client.GetAsync(
            new Uri("/api/seo/canonical/en/destinations/live", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal(HttpStatusCode.OK, canonical.StatusCode);
        var body = await canonical.Content.ReadAsStringAsync(ct);
        Assert.Contains("destinations/live", body, StringComparison.Ordinal);
        Assert.Contains("isSelfCanonical", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolve_DoesNotCrossLocale()
    {
        await using var factory = _fixture.CreateFactory();
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-000000000205");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var routes = scope.ServiceProvider.GetRequiredService<ISeoRouteService>();
            var created = await routes.CreateAsync(
                new CreateSeoRouteRequest("Destination", resourceId, "fa", "destinations/old-fa"),
                ct);
            await routes.ChangePathAsync(
                created.Id,
                new ChangeSeoRoutePathRequest("destinations/new-fa"),
                ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var enLookup = await client.GetAsync(
            new Uri("/api/seo/resolve/en/destinations/old-fa", UriKind.Relative),
            ct);
        var faLookup = await client.GetAsync(
            new Uri("/api/seo/resolve/fa/destinations/old-fa", UriKind.Relative),
            ct);

        Assert.Equal(HttpStatusCode.NotFound, enLookup.StatusCode);
        Assert.Equal(HttpStatusCode.MovedPermanently, faLookup.StatusCode);
        Assert.Equal("/fa/destinations/new-fa", faLookup.Headers.Location?.ToString());
    }
}
