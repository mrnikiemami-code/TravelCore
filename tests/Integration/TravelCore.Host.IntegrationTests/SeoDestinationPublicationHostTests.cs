using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
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
public sealed class SeoDestinationPublicationHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IdentityAuthHostFixture _fixture;

    public SeoDestinationPublicationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Publish_RequiresAuth_ThenCreatesSeoRoute_Idempotent_AndConflictOnOtherResource()
    {
        await using var factory = _fixture.CreateFactory(Environments.Development);
        var ct = TestContext.Current.CancellationToken;
        var destA = Guid.Parse("0198a000-0000-7000-8000-000000000901");
        var destB = Guid.Parse("0198a000-0000-7000-8000-000000000902");
        const string email = "seo-publish-authz@travelcore.test";
        const string password = "Seo-Publish-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
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

        var denied = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            var admin = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode);
            adminRoleId = admin.Id.Value;
            Assert.Contains(
                accessDb.Permissions.AsEnumerable(),
                x => x.Code == "seo.destination-posture.write");
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

        var created = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(createdBody);
        Assert.True(createdBody.Created);
        Assert.Equal("destinations/istanbul", createdBody.PublicPath);

        var again = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul"),
            ct);
        var againBody = await again.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(againBody);
        Assert.False(againBody.Created);
        Assert.False(againBody.PathChanged);

        var conflict = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destB, "en", "istanbul"),
            ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        var changed = await client.PostAsJsonAsync(
            new Uri("/api/seo/publication/destination", UriKind.Relative),
            new PublishDestinationSeoRouteRequest(destA, "en", "istanbul-city"),
            ct);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var changedBody = await changed.Content.ReadFromJsonAsync<PublishDestinationSeoRouteResponse>(JsonOptions, ct);
        Assert.NotNull(changedBody);
        Assert.True(changedBody.PathChanged);
        Assert.Equal("destinations/istanbul-city", changedBody.PublicPath);

        // IndexPolicy still missing → compose remains noindex (R2; no mass flip on publish).
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var metadata = scope.ServiceProvider.GetRequiredService<ISeoMetadataService>();
            var composed = await metadata.ComposeAsync(
                new ComposeSeoMetadataRequest("en", "destinations/istanbul-city", "Istanbul", null),
                ct);
            Assert.Equal("NoIndex", composed.EffectiveIndex);
            Assert.False(composed.IsIndexable);
        }
    }
}
