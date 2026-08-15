using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.ReferenceData.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class DestinationAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public DestinationAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Destination_create_enforces_authn_and_access_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "destination-authz@travelcore.test";
        const string password = "Destination-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using (var referenceData = _fixture.CreateReferenceDataDb())
        {
            await ReferenceDataMigrator.MigrateAsync(referenceData, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createBody = new CreateDestinationRequest(
            Kind: "Country",
            Code: $"IR-AUTHZ-{Guid.NewGuid():N}"[..20],
            EnglishName: "Iran Authz",
            ParentId: null,
            IsoCountryCode: "IR");

        var anonymous = await client.PostAsJsonAsync("/api/destination/destinations/", createBody, ct);
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

        var authenticatedDenied = await client.PostAsJsonAsync("/api/destination/destinations/", createBody, ct);
        Assert.Equal(HttpStatusCode.Forbidden, authenticatedDenied.StatusCode);

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            var admin = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode);
            adminRoleId = admin.Id.Value;
            Assert.Contains(
                accessDb.Permissions.AsEnumerable(),
                x => x.Code == "destination.destinations.write");
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

        var allowed = await client.PostAsJsonAsync("/api/destination/destinations/", createBody, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
    }
}
