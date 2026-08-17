using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class PricingPublicQueryHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public PricingPublicQueryHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_price_summary_is_anonymous_read_only_and_omits_agency_occupancy()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "pricing-public@travelcore.test";
        const string password = "Pricing-Public-Password-1";
        var targetId = Guid.Parse("01900000-0000-7000-8000-0000000000c8");

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var anonymousClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var adminClient = factory.CreateClient(new() { AllowAutoRedirect = false });

        var missing = await anonymousClient.GetAsync(
            $"/api/pricing/public/tour-departures/{targetId:D}",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var genericMissing = await anonymousClient.GetAsync(
            $"/api/pricing/public/summaries?targetType=TourDeparture&targetId={targetId:D}",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, genericMissing.StatusCode);

        var createAccount = await adminClient.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAccount.StatusCode);
        using var createDoc = JsonDocument.Parse(await createAccount.Content.ReadAsStringAsync(ct));
        var accountId = createDoc.RootElement.GetProperty("id").GetGuid();

        var login = await adminClient.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            adminRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode).Id.Value;
        }

        var assign = await adminClient.PostAsJsonAsync(
            "/api/access/subject-roles/",
            new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = accountId,
                RoleId = adminRoleId
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);

        var createBody = new CreatePriceRequest(
            TargetType: "TourDeparture",
            TargetId: targetId,
            Components:
            [
                new PriceComponentInput("Base", new MoneyInput(1290m, "USD"), SortOrder: 0, Code: "BASE")
            ],
            OccupancyRules:
            [
                new PriceOccupancyRuleInput("Public", "Adult", "SingleRoom", new MoneyInput(1290m, "USD"), SortOrder: 0),
                new PriceOccupancyRuleInput("Agency", "Adult", "SingleRoom", new MoneyInput(1100m, "USD"), SortOrder: 1)
            ]);

        var created = await adminClient.PostAsJsonAsync("/api/pricing/prices/", createBody, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var priceId = createdDoc.RootElement.GetProperty("id").GetGuid();

        var publicGet = await anonymousClient.GetAsync(
            $"/api/pricing/public/tour-departures/{targetId:D}",
            ct);
        Assert.Equal(HttpStatusCode.OK, publicGet.StatusCode);
        using var publicDoc = JsonDocument.Parse(await publicGet.Content.ReadAsStringAsync(ct));
        var root = publicDoc.RootElement;
        Assert.Equal(priceId, root.GetProperty("priceId").GetGuid());
        Assert.Equal("TourDeparture", root.GetProperty("targetType").GetString());
        Assert.Equal(targetId, root.GetProperty("targetId").GetGuid());
        Assert.Equal("USD", root.GetProperty("currency").GetString());
        Assert.False(root.TryGetProperty("convertedAmount", out _));
        Assert.False(root.TryGetProperty("displayAmount", out _));
        Assert.False(root.TryGetProperty("exchangeRate", out _));

        var components = root.GetProperty("components");
        Assert.Equal(1, components.GetArrayLength());
        Assert.Equal("Base", components[0].GetProperty("kind").GetString());
        Assert.Equal(1290m, components[0].GetProperty("money").GetProperty("amount").GetDecimal());
        Assert.Equal("USD", components[0].GetProperty("money").GetProperty("currencyCode").GetString());

        var occupancy = root.GetProperty("occupancyPrices");
        Assert.Equal(1, occupancy.GetArrayLength());
        Assert.Equal("Adult", occupancy[0].GetProperty("passengerCategory").GetString());
        Assert.Equal("SingleRoom", occupancy[0].GetProperty("occupancyCategory").GetString());
        Assert.Equal(1290m, occupancy[0].GetProperty("money").GetProperty("amount").GetDecimal());

        var genericGet = await anonymousClient.GetAsync(
            $"/api/pricing/public/summaries?targetType=TourDeparture&targetId={targetId:D}",
            ct);
        Assert.Equal(HttpStatusCode.OK, genericGet.StatusCode);

        var adminStillProtected = await anonymousClient.GetAsync($"/api/pricing/prices/{priceId:D}", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, adminStillProtected.StatusCode);

        var publicPost = await anonymousClient.PostAsJsonAsync(
            $"/api/pricing/public/tour-departures/{targetId:D}",
            new { },
            ct);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, publicPost.StatusCode);
    }
}
