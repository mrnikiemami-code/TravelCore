using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Media.Domain;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class MediaAssetListAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public MediaAssetListAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Media_asset_list_enforces_authn_authz_and_filters_by_status()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-list-authz@travelcore.test";
        const string password = "Media-List-Authz-Password-1";
        var readyId = Guid.Parse("66666666-6666-7666-8666-666666666666");
        var failedId = Guid.Parse("77777777-7777-7777-8777-777777777777");

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "media.assets.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.GetAsync("/api/media/assets/?take=10", ct);
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

        var authenticatedDenied = await client.GetAsync("/api/media/assets/?take=10", ct);
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

        await using (var mediaDb = _fixture.CreateMediaDb())
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            mediaDb.MediaAssets.Add(MediaAsset.Create(
                "image/png",
                10,
                now,
                id: MediaAssetId.From(readyId),
                status: MediaAssetStatus.Ready));
            mediaDb.MediaAssets.Add(MediaAsset.Create(
                "image/jpeg",
                20,
                now,
                id: MediaAssetId.From(failedId),
                status: MediaAssetStatus.Failed));
            await mediaDb.SaveChangesAsync(ct);
        }

        var allowed = await client.GetAsync("/api/media/assets/?take=50", ct);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        using var listDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal(JsonValueKind.Array, listDoc.RootElement.ValueKind);
        Assert.True(listDoc.RootElement.GetArrayLength() >= 2);

        var readyOnly = await client.GetAsync("/api/media/assets/?status=Ready&take=50", ct);
        Assert.Equal(HttpStatusCode.OK, readyOnly.StatusCode);
        using var readyDoc = JsonDocument.Parse(await readyOnly.Content.ReadAsStringAsync(ct));
        Assert.All(readyDoc.RootElement.EnumerateArray(), el =>
            Assert.Equal("Ready", el.GetProperty("status").GetString()));

        var badStatus = await client.GetAsync("/api/media/assets/?status=Published&take=10", ct);
        Assert.Equal(HttpStatusCode.BadRequest, badStatus.StatusCode);
    }
}
