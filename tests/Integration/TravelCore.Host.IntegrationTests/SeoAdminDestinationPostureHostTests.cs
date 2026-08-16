using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Seo.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class SeoAdminDestinationPostureHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IdentityAuthHostFixture _fixture;

    public SeoAdminDestinationPostureHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Admin_destination_posture_and_index_policy_are_access_backed()
    {
        var ct = TestContext.Current.CancellationToken;
        var destinationId = Guid.Parse("0198a000-0000-7000-8000-000000000a11");
        const string email = "seo-admin-posture@travelcore.test";
        const string password = "Seo-Admin-Posture-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymousPosture = await client.GetAsync(
            new Uri($"/api/seo/admin/destination-posture/{destinationId}/en", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousPosture.StatusCode);

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

        var forbidden = await client.GetAsync(
            new Uri($"/api/seo/admin/destination-posture/{destinationId}/en", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

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

        var publish = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destinationId, "en", "tehran-admin"),
            ct);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var postureBefore = await client.GetAsync(
            new Uri($"/api/seo/admin/destination-posture/{destinationId}/en", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.OK, postureBefore.StatusCode);
        var beforeBody = await postureBefore.Content.ReadFromJsonAsync<SeoDestinationPostureResponse>(JsonOptions, ct);
        Assert.NotNull(beforeBody);
        Assert.Single(beforeBody.Routes);
        Assert.Null(beforeBody.ConfiguredPolicy);
        Assert.NotNull(beforeBody.EffectiveIndexability);
        Assert.False(beforeBody.EffectiveIndexability.IsIndexable);
        Assert.Contains("noindex", beforeBody.EffectiveIndexability.RobotsDirective, StringComparison.OrdinalIgnoreCase);

        var setPolicy = await client.PutAsJsonAsync(
            new Uri("/api/seo/index-policies", UriKind.Relative),
            new SetSeoIndexPolicyRequest(
                "Destination",
                destinationId,
                "en",
                "Index",
                "Follow"),
            ct);
        Assert.Equal(HttpStatusCode.OK, setPolicy.StatusCode);

        var postureAfter = await client.GetAsync(
            new Uri($"/api/seo/admin/destination-posture/{destinationId}/en", UriKind.Relative),
            ct);
        Assert.Equal(HttpStatusCode.OK, postureAfter.StatusCode);
        var afterBody = await postureAfter.Content.ReadFromJsonAsync<SeoDestinationPostureResponse>(JsonOptions, ct);
        Assert.NotNull(afterBody);
        Assert.NotNull(afterBody.ConfiguredPolicy);
        Assert.Equal("Index", afterBody.ConfiguredPolicy.IndexDirective);
        Assert.NotNull(afterBody.EffectiveIndexability);
        Assert.True(afterBody.EffectiveIndexability.IsIndexable);
        Assert.Contains("index", afterBody.EffectiveIndexability.RobotsDirective, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("noindex", afterBody.EffectiveIndexability.RobotsDirective, StringComparison.OrdinalIgnoreCase);
    }
}
