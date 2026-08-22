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
using TravelCore.Modules.Party.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

/// <summary>
/// Admin AgencyOffer governance access + self-moderation (TC-P38-T010).
/// </summary>
[Collection(nameof(IdentityAuthHostCollection))]
public sealed class AgencyOfferGovernanceAccessTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public AgencyOfferGovernanceAccessTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Agency_role_cannot_approve_and_admin_moderation_queue_works()
    {
        var ct = TestContext.Current.CancellationToken;
        const string agencyEmail = "agency-offer-gov-agency@travelcore.test";
        const string adminEmail = "agency-offer-gov-admin@travelcore.test";
        const string password = "Agency-Offer-Gov-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var agencyClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var adminClient = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createParty = await agencyClient.PostAsJsonAsync(
            "/api/party/parties/",
            new CreatePartyRequest
            {
                Kind = "Agency",
                DisplayName = "Gov Agency",
                TradingName = "Gov Agency Trading"
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, createParty.StatusCode);
        using var partyDoc = JsonDocument.Parse(await createParty.Content.ReadAsStringAsync(ct));
        var partyId = partyDoc.RootElement.GetProperty("id").GetGuid();

        var createAgencyAccount = await agencyClient.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest
            {
                Email = agencyEmail,
                Password = password,
                AssociatedPartyId = partyId
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAgencyAccount.StatusCode);
        using var agencyAccountDoc = JsonDocument.Parse(await createAgencyAccount.Content.ReadAsStringAsync(ct));
        var agencyAccountId = agencyAccountDoc.RootElement.GetProperty("id").GetGuid();

        var agencyLogin = await agencyClient.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = agencyEmail, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, agencyLogin.StatusCode);

        Guid agencyRoleId;
        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            agencyRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AgencyRoleCode).Id.Value;
            adminRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode).Id.Value;
        }

        var assignAgency = await agencyClient.PostAsJsonAsync(
            "/api/access/subject-roles/",
            new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = agencyAccountId,
                RoleId = agencyRoleId
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, assignAgency.StatusCode);

        var profileBody = new UpsertAgencyProfileRequest(PartyId: partyId, DisplayName: "Gov Agency Profile");
        var profile = await agencyClient.PostAsJsonAsync("/api/agency-marketplace/profiles/", profileBody, ct);
        Assert.Equal(HttpStatusCode.Created, profile.StatusCode);
        using var profileDoc = JsonDocument.Parse(await profile.Content.ReadAsStringAsync(ct));
        var profileId = profileDoc.RootElement.GetProperty("id").GetGuid();

        var offerBody = new CreateAgencyOfferRequest(
            AgencyProfileId: profileId,
            TourProductId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000c1"));
        var created = await agencyClient.PostAsJsonAsync("/api/agency-marketplace/offers/", offerBody, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var offerId = createdDoc.RootElement.GetProperty("id").GetGuid();

        var submit = await agencyClient.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/submit", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);

        var agencyApprove = await agencyClient.PostAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/approve",
            null,
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, agencyApprove.StatusCode);

        var agencyLegacyApprove = await agencyClient.PostAsync(
            $"/api/agency-marketplace/offers/{offerId:D}/approve",
            null,
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, agencyLegacyApprove.StatusCode);

        var createAdminAccount = await adminClient.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = adminEmail, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAdminAccount.StatusCode);
        using var adminAccountDoc = JsonDocument.Parse(await createAdminAccount.Content.ReadAsStringAsync(ct));
        var adminAccountId = adminAccountDoc.RootElement.GetProperty("id").GetGuid();

        var adminLogin = await adminClient.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = adminEmail, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);

        var assignAdmin = await adminClient.PostAsJsonAsync(
            "/api/access/subject-roles/",
            new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = adminAccountId,
                RoleId = adminRoleId
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, assignAdmin.StatusCode);

        var pending = await adminClient.GetAsync(
            "/api/agency-marketplace/moderation/offers/pending?take=50",
            ct);
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
        using var pendingDoc = JsonDocument.Parse(await pending.Content.ReadAsStringAsync(ct));
        Assert.Contains(
            pendingDoc.RootElement.EnumerateArray(),
            x => x.GetProperty("offerId").GetGuid() == offerId);

        var approve = await adminClient.PostAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/approve",
            null,
            ct);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        using var approveDoc = JsonDocument.Parse(await approve.Content.ReadAsStringAsync(ct));
        Assert.Equal("Approved", approveDoc.RootElement.GetProperty("publicationStatus").GetString());
        Assert.False(approveDoc.RootElement.TryGetProperty("commission", out _));

        var publish = await agencyClient.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/publish", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, publish.StatusCode);

        var suspend = await adminClient.PostAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/suspend",
            null,
            ct);
        Assert.Equal(HttpStatusCode.OK, suspend.StatusCode);
        using var suspendDoc = JsonDocument.Parse(await suspend.Content.ReadAsStringAsync(ct));
        Assert.Equal("Suspended", suspendDoc.RootElement.GetProperty("publicationStatus").GetString());
        Assert.Equal("Unlisted", suspendDoc.RootElement.GetProperty("visibility").GetString());

        var approvedFilter = await adminClient.GetAsync(
            "/api/agency-marketplace/moderation/offers?publicationStatus=Approved&take=50",
            ct);
        Assert.Equal(HttpStatusCode.OK, approvedFilter.StatusCode);
        using var approvedDoc = JsonDocument.Parse(await approvedFilter.Content.ReadAsStringAsync(ct));
        Assert.DoesNotContain(
            approvedDoc.RootElement.EnumerateArray(),
            x => x.GetProperty("offerId").GetGuid() == offerId);

        var suspendedFilter = await adminClient.GetAsync(
            "/api/agency-marketplace/moderation/offers?publicationStatus=Suspended&take=50",
            ct);
        Assert.Equal(HttpStatusCode.OK, suspendedFilter.StatusCode);
        using var suspendedDoc = JsonDocument.Parse(await suspendedFilter.Content.ReadAsStringAsync(ct));
        var suspendedItem = Assert.Single(
            suspendedDoc.RootElement.EnumerateArray(),
            x => x.GetProperty("offerId").GetGuid() == offerId);
        Assert.Equal("Suspended", suspendedItem.GetProperty("publicationStatus").GetString());
        Assert.True(suspendedItem.GetProperty("hasGovernanceHistory").GetBoolean());
        Assert.Equal("Suspended", suspendedItem.GetProperty("lastDecisionKind").GetString());
        Assert.False(suspendedItem.TryGetProperty("commission", out _));
        Assert.False(suspendedItem.TryGetProperty("revenue", out _));

        var badFilter = await adminClient.GetAsync(
            "/api/agency-marketplace/moderation/offers?publicationStatus=Published&take=10",
            ct);
        Assert.Equal(HttpStatusCode.BadRequest, badFilter.StatusCode);

        var history = await adminClient.GetAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/governance-history?take=20",
            ct);
        Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        using var historyDoc = JsonDocument.Parse(await history.Content.ReadAsStringAsync(ct));
        var kinds = historyDoc.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("kind").GetString())
            .ToList();
        Assert.Contains("Submitted", kinds);
        Assert.Contains("Approved", kinds);
        Assert.Contains("Published", kinds);
        Assert.Contains("Suspended", kinds);
        Assert.All(
            historyDoc.RootElement.EnumerateArray(),
            x =>
            {
                Assert.False(x.TryGetProperty("commission", out _));
                Assert.False(x.TryGetProperty("settlement", out _));
            });

        var agencyHistory = await agencyClient.GetAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/governance-history",
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, agencyHistory.StatusCode);
    }

    [Fact]
    public async Task Admin_with_owning_agency_profile_cannot_self_approve()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "agency-offer-gov-self@travelcore.test";
        const string password = "Agency-Offer-Gov-Self-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createParty = await client.PostAsJsonAsync(
            "/api/party/parties/",
            new CreatePartyRequest
            {
                Kind = "Agency",
                DisplayName = "Self Gov Agency",
                TradingName = "Self Gov Trading"
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

        var profile = await client.PostAsJsonAsync(
            "/api/agency-marketplace/profiles/",
            new UpsertAgencyProfileRequest(PartyId: partyId, DisplayName: "Self Profile"),
            ct);
        Assert.Equal(HttpStatusCode.Created, profile.StatusCode);
        using var profileDoc = JsonDocument.Parse(await profile.Content.ReadAsStringAsync(ct));
        var profileId = profileDoc.RootElement.GetProperty("id").GetGuid();

        var created = await client.PostAsJsonAsync(
            "/api/agency-marketplace/offers/",
            new CreateAgencyOfferRequest(
                AgencyProfileId: profileId,
                TourProductId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000c2")),
            ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var offerId = createdDoc.RootElement.GetProperty("id").GetGuid();

        var submit = await client.PostAsync($"/api/agency-marketplace/offers/{offerId:D}/submit", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);

        var selfApprove = await client.PostAsync(
            $"/api/agency-marketplace/moderation/offers/{offerId:D}/approve",
            null,
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, selfApprove.StatusCode);
    }
}
