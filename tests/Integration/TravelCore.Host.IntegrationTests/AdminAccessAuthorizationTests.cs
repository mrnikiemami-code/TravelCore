using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class AdminAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public AdminAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Admin_roles_endpoint_enforces_authn_and_access_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "admin-authz@travelcore.test";
        const string password = "Admin-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.GetAsync(new Uri("/api/admin/access/roles", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var create = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createDoc = JsonDocument.Parse(await create.Content.ReadAsStringAsync(ct));
        var accountId = createDoc.RootElement.GetProperty("id").GetGuid();

        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var authenticatedDenied = await client.GetAsync(new Uri("/api/admin/access/roles", UriKind.Relative), ct);
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

        var allowed = await client.GetAsync(new Uri("/api/admin/access/roles", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        using var rolesDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.True(rolesDoc.RootElement.GetArrayLength() >= 1);
    }
}
