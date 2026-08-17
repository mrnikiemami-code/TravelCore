using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Tour.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class TourDepartureAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public TourDepartureAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Tour_departure_create_enforces_authn_and_access_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "tour-departure-authz@travelcore.test";
        const string password = "Tour-Departure-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "tour.departures.write");
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "tour.departures.read");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Seed a product first with admin (after account + role).
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

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            var admin = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode);
            adminRoleId = admin.Id.Value;
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

        var createProduct = await client.PostAsJsonAsync(
            "/api/tour/products/",
            new CreateTourProductRequest(
                Kind: "Package",
                Code: $"pkg-dep-{Guid.NewGuid():N}"[..20],
                EnglishName: "Departure Authz Package"),
            ct);
        Assert.Equal(HttpStatusCode.Created, createProduct.StatusCode);
        using var productDoc = JsonDocument.Parse(await createProduct.Content.ReadAsStringAsync(ct));
        var productId = productDoc.RootElement.GetProperty("id").GetGuid();

        // Fresh client without auth for anonymous / pre-role checks would need separate flow;
        // use logout-style by creating a second unauthenticated client.
        using var anonymousClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var createBody = new CreateTourDepartureRequest(productId);

        var anonymous = await anonymousClient.PostAsJsonAsync("/api/tour/departures/", createBody, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        // Authenticated without role: new account
        const string email2 = "tour-departure-denied@travelcore.test";
        using var deniedClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var createDeniedAccount = await deniedClient.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email2, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, createDeniedAccount.StatusCode);
        var loginDenied = await deniedClient.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email2, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, loginDenied.StatusCode);

        var authenticatedDenied = await deniedClient.PostAsJsonAsync("/api/tour/departures/", createBody, ct);
        Assert.Equal(HttpStatusCode.Forbidden, authenticatedDenied.StatusCode);

        var allowed = await client.PostAsJsonAsync("/api/tour/departures/", createBody, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        using var allowedDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal(productId, allowedDoc.RootElement.GetProperty("tourProductId").GetGuid());
        Assert.Equal("Draft", allowedDoc.RootElement.GetProperty("status").GetString());

        var list = await client.GetAsync($"/api/tour/departures/?tourProductId={productId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
    }
}
