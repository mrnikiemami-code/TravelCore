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
public sealed class SeoTourProductPublicationHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IdentityAuthHostFixture _fixture;

    public SeoTourProductPublicationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_TourProduct_RequiresAuth_ThenCreatesSeoRoute_Idempotent()
    {
        await using var factory = _fixture.CreateFactory(Environments.Development);
        var ct = TestContext.Current.CancellationToken;
        var tourA = Guid.Parse("0198b000-0000-7000-8000-000000000b01");
        var tourB = Guid.Parse("0198b000-0000-7000-8000-000000000b02");
        const string email = "seo-tour-publish@travelcore.test";
        const string password = "Seo-Tour-Publish-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "seo.tour-posture.write");
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/tour-product", UriKind.Relative),
            new PublishTourProductSeoRouteRequest(tourA, "en", "caspian-walk"),
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
            new Uri("/api/seo/publication/tour-product", UriKind.Relative),
            new PublishTourProductSeoRouteRequest(tourA, "en", "caspian-walk"),
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
            new Uri("/api/seo/publication/tour-product", UriKind.Relative),
            new PublishTourProductSeoRouteRequest(tourA, "en", "caspian-walk"),
            ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<PublishTourProductSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(createdBody);
        Assert.True(createdBody.Created);
        Assert.Equal("tours/caspian-walk", createdBody.PublicPath);
        Assert.Equal("TourProduct", createdBody.Route.ResourceType);

        var again = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/tour-product", UriKind.Relative),
            new PublishTourProductSeoRouteRequest(tourA, "en", "caspian-walk"),
            ct);
        var againBody = await again.Content.ReadFromJsonAsync<PublishTourProductSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(againBody);
        Assert.False(againBody.Created);
        Assert.False(againBody.PathChanged);

        var conflict = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/tour-product", UriKind.Relative),
            new PublishTourProductSeoRouteRequest(tourB, "en", "caspian-walk"),
            ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }
}
