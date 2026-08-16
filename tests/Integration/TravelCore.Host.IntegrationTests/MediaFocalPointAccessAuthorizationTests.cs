using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class MediaFocalPointAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public MediaFocalPointAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Media_focal_point_enforces_authn_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-focal-authz@travelcore.test";
        const string password = "Media-Focal-Authz-Password-1";
        var assetId = Guid.Parse("33333333-3333-7333-8333-333333333333");

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "media.assets.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/focal-point",
            new UpsertFocalPointRequest(0.5, 0.5),
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

        var authenticatedDenied = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/focal-point",
            new UpsertFocalPointRequest(0.5, 0.5),
            ct);
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
            var asset = MediaAsset.Create(
                "image/png",
                10,
                now,
                id: MediaAssetId.From(assetId),
                status: MediaAssetStatus.Ready);
            mediaDb.MediaAssets.Add(asset);
            await mediaDb.SaveChangesAsync(ct);
        }

        var allowedPut = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/focal-point",
            new UpsertFocalPointRequest(0.33, 0.66),
            ct);
        Assert.Equal(HttpStatusCode.OK, allowedPut.StatusCode);
        using var putDoc = JsonDocument.Parse(await allowedPut.Content.ReadAsStringAsync(ct));
        Assert.Equal(0.33, putDoc.RootElement.GetProperty("focalX").GetDouble());
        Assert.Equal(0.66, putDoc.RootElement.GetProperty("focalY").GetDouble());

        var allowedGet = await client.GetAsync($"/api/media/assets/{assetId:D}/focal-point", ct);
        Assert.Equal(HttpStatusCode.OK, allowedGet.StatusCode);
        using var getDoc = JsonDocument.Parse(await allowedGet.Content.ReadAsStringAsync(ct));
        Assert.Equal(0.33, getDoc.RootElement.GetProperty("focalX").GetDouble());
        Assert.Equal(0.66, getDoc.RootElement.GetProperty("focalY").GetDouble());

        var assetGet = await client.GetAsync($"/api/media/assets/{assetId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, assetGet.StatusCode);
        using var assetDoc = JsonDocument.Parse(await assetGet.Content.ReadAsStringAsync(ct));
        Assert.Equal(0.33, assetDoc.RootElement.GetProperty("focalX").GetDouble());
        Assert.Equal(0.66, assetDoc.RootElement.GetProperty("focalY").GetDouble());
    }
}
