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
public sealed class PricingAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public PricingAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Pricing_price_create_enforces_authn_and_access_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "pricing-authz@travelcore.test";
        const string password = "Pricing-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "pricing.prices.write");
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "pricing.prices.read");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createBody = new CreatePriceRequest(
            TargetType: "TourDeparture",
            TargetId: Guid.Parse("01900000-0000-7000-8000-0000000000bb"),
            Components:
            [
                new PriceComponentInput("Base", new MoneyInput(50m, "USD"), SortOrder: 0, Code: "BASE")
            ]);

        using var anonymousClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var anonymous = await anonymousClient.PostAsJsonAsync("/api/pricing/prices/", createBody, ct);
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

        var authenticatedDenied = await client.PostAsJsonAsync("/api/pricing/prices/", createBody, ct);
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

        var allowed = await client.PostAsJsonAsync("/api/pricing/prices/", createBody, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        using var allowedDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal("TourDeparture", allowedDoc.RootElement.GetProperty("targetType").GetString());
        Assert.Equal("USD", allowedDoc.RootElement.GetProperty("currencyCode").GetString());
        var priceId = allowedDoc.RootElement.GetProperty("id").GetGuid();

        var get = await client.GetAsync($"/api/pricing/prices/{priceId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        using var readDeniedClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var anonymousGet = await readDeniedClient.GetAsync($"/api/pricing/prices/{priceId:D}", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
    }
}
