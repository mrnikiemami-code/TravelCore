using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Place.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class PlaceAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public PlaceAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Place_create_enforces_authn_and_access_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "place-authz@travelcore.test";
        const string password = "Place-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "place.places.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createBody = new CreatePlaceRequest(
            Kind: "Hotel",
            Code: $"htl-authz-{Guid.NewGuid():N}"[..20],
            EnglishName: "Authz Hotel",
            StarRating: 4);

        var anonymous = await client.PostAsJsonAsync("/api/place/places/", createBody, ct);
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

        var authenticatedDenied = await client.PostAsJsonAsync("/api/place/places/", createBody, ct);
        Assert.Equal(HttpStatusCode.Forbidden, authenticatedDenied.StatusCode);

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

        var allowed = await client.PostAsJsonAsync("/api/place/places/", createBody, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        using var allowedDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal("Hotel", allowedDoc.RootElement.GetProperty("kind").GetString());
        Assert.Equal("Draft", allowedDoc.RootElement.GetProperty("catalogStatus").GetString());
    }
}
