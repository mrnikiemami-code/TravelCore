using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class SeoContentPublicationHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IdentityAuthHostFixture _fixture;

    public SeoContentPublicationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_Article_RequiresAuth_ThenCreatesSeoRoute_Idempotent()
    {
        await using var factory = _fixture.CreateFactory(Environments.Development);
        var ct = TestContext.Current.CancellationToken;
        var articleA = Guid.Parse("0198a000-0000-7000-8000-000000000b01");
        var articleB = Guid.Parse("0198a000-0000-7000-8000-000000000b02");
        const string email = "seo-content-publish@travelcore.test";
        const string password = "Seo-Content-Publish-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "seo.content-posture.write");
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/article", UriKind.Relative),
            new PublishArticleSeoRouteRequest(articleA, "en", "summer-tips"),
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var createAccount = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAccount.StatusCode);
        using var createDoc = JsonDocument.Parse(await createAccount.Content.ReadAsStringAsync(ct));
        var accountId = createDoc.RootElement.GetProperty("id").GetGuid();

        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var denied = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/article", UriKind.Relative),
            new PublishArticleSeoRouteRequest(articleA, "en", "summer-tips"),
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            adminRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode).Id.Value;
        }

        var assign = await client.PostAsJsonAsync(
            "/api/access/subject-roles/",
            new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = accountId,
                RoleId = adminRoleId
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);

        var created = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/article", UriKind.Relative),
            new PublishArticleSeoRouteRequest(articleA, "en", "summer-tips"),
            ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<PublishArticleSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(createdBody);
        Assert.True(createdBody.Created);
        Assert.Equal("articles/summer-tips", createdBody.PublicPath);
        Assert.Equal("Article", createdBody.Route.ResourceType);

        var again = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/article", UriKind.Relative),
            new PublishArticleSeoRouteRequest(articleA, "en", "summer-tips"),
            ct);
        var againBody = await again.Content.ReadFromJsonAsync<PublishArticleSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(againBody);
        Assert.False(againBody.Created);
        Assert.False(againBody.PathChanged);

        var conflict = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/article", UriKind.Relative),
            new PublishArticleSeoRouteRequest(articleB, "en", "summer-tips"),
            ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        var landing = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/landing-page", UriKind.Relative),
            new PublishLandingPageSeoRouteRequest(articleA, "en", "promo-home"),
            ct);
        Assert.Equal(HttpStatusCode.OK, landing.StatusCode);
        var landingBody = await landing.Content.ReadFromJsonAsync<PublishLandingPageSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(landingBody);
        Assert.True(landingBody.Created);
        Assert.Equal("landing-pages/promo-home", landingBody.PublicPath);
        Assert.Equal("LandingPage", landingBody.Route.ResourceType);
    }
}
