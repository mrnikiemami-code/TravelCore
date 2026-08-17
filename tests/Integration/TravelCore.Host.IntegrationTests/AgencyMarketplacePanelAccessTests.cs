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

    [Fact]
    public async Task Agency_offer_publication_lifecycle_is_marketplace_owned_not_seo()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "agency-marketplace-publish@travelcore.test";
        const string password = "Agency-Marketplace-Publish-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "agency.marketplace.offers.moderate");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using var anonymousClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var anonymousSubmit = await anonymousClient.PostAsync(
            $"/api/agency-marketplace/offers/{Guid.NewGuid():D}/submit",
            null,
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousSubmit.StatusCode);

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

        var profileBody = new UpsertAgencyProfileRequest(
            PartyId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000bb"),
            DisplayName: "Publish Agency");
        var profile = await client.PostAsJsonAsync("/api/agency-marketplace/profiles/", profileBody, ct);
        Assert.Equal(HttpStatusCode.Created, profile.StatusCode);
        using var profileDoc = JsonDocument.Parse(await profile.Content.ReadAsStringAsync(ct));
        var profileId = profileDoc.RootElement.GetProperty("id").GetGuid();

        var offerBody = new CreateAgencyOfferRequest(
            AgencyProfileId: profileId,
            TourProductId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000cc"));
        var created = await client.PostAsJsonAsync("/api/agency-marketplace/offers/", offerBody, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var offerId = createdDoc.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Draft", createdDoc.RootElement.GetProperty("publicationStatus").GetString());
        Assert.False(createdDoc.RootElement.TryGetProperty("indexPolicy", out _));
        Assert.False(createdDoc.RootElement.TryGetProperty("catalogStatus", out _));

        var submit = await client.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/submit", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);

        var approve = await client.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/approve", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var publish = await client.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/publish", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, publish.StatusCode);

        var listed = await client.GetAsync($"/api/agency-marketplace/offers/?agencyProfileId={profileId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
        using var listedDoc = JsonDocument.Parse(await listed.Content.ReadAsStringAsync(ct));
        var item = listedDoc.RootElement.EnumerateArray().Single();
        Assert.Equal("Published", item.GetProperty("publicationStatus").GetString());
        Assert.Equal("Listed", item.GetProperty("visibility").GetString());
        Assert.False(item.TryGetProperty("indexPolicy", out _));
    }
}
