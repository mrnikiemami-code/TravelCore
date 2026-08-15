using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Party.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class AgencyPresentationAccessTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public AgencyPresentationAccessTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Agency_panel_requires_auth_permission_and_agency_party()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "agency-panel@travelcore.test";
        const string password = "Agency-Panel-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.GetAsync(new Uri("/api/agency/panel/capabilities", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var createParty = await client.PostAsJsonAsync(
            "/api/party/parties/",
            new CreatePartyRequest
            {
                Kind = "Agency",
                DisplayName = "Demo Agency",
                TradingName = "Demo Agency Trading"
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, createParty.StatusCode);
        using var partyDoc = JsonDocument.Parse(await createParty.Content.ReadAsStringAsync(ct));
        var partyId = partyDoc.RootElement.GetProperty("id").GetGuid();

        var createAccount = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest
            {
                Email = email,
                Password = password,
                AssociatedPartyId = partyId
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAccount.StatusCode);
        using var accountDoc = JsonDocument.Parse(await createAccount.Content.ReadAsStringAsync(ct));
        var accountId = accountDoc.RootElement.GetProperty("id").GetGuid();

        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var forbidden = await client.GetAsync(new Uri("/api/agency/panel/capabilities", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        Guid agencyRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            agencyRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AgencyRoleCode).Id.Value;
        }

        var assign = await client.PostAsJsonAsync(
            "/api/access/subject-roles/",
            new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = accountId,
                RoleId = agencyRoleId
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);

        var allowed = await client.GetAsync(new Uri("/api/agency/panel/capabilities", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        using var body = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.False(body.RootElement.GetProperty("commerceEnabled").GetBoolean());
        Assert.False(body.RootElement.GetProperty("tourOwned").GetBoolean());
        Assert.Equal("Agency", body.RootElement.GetProperty("actingParty").GetProperty("kind").GetString());
    }
}
