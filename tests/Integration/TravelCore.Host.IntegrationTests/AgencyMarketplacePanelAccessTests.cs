using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Identity.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class AgencyMarketplacePanelAccessTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public AgencyMarketplacePanelAccessTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Agency_marketplace_profile_upsert_enforces_authn_and_access_authz()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "agency-marketplace-panel@travelcore.test";
        const string password = "Agency-Marketplace-Panel-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "agency.marketplace.profile.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var body = new UpsertAgencyProfileRequest(
            PartyId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000aa"),
            DisplayName: "Panel Agency");

        using var anonymousClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var anonymous = await anonymousClient.PostAsJsonAsync("/api/agency-marketplace/profiles/", body, ct);
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

        var denied = await client.PostAsJsonAsync("/api/agency-marketplace/profiles/", body, ct);
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

        var allowed = await client.PostAsJsonAsync("/api/agency-marketplace/profiles/", body, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        using var allowedDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal("Panel Agency", allowedDoc.RootElement.GetProperty("displayName").GetString());
        Assert.False(allowedDoc.RootElement.TryGetProperty("commission", out _));
    }
}
