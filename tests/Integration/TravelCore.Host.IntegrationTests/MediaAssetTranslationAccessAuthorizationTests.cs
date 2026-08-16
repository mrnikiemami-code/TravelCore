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
public sealed class MediaAssetTranslationAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public MediaAssetTranslationAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Media_translations_enforces_authn_authz_and_presentation_rules()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-alt-authz@travelcore.test";
        const string password = "Media-Alt-Authz-Password-1";
        var assetId = Guid.Parse("55555555-5555-7555-8555-555555555555");

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "media.assets.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/translations/fa",
            new UpsertMediaAssetTranslationRequest("alt"),
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
            $"/api/media/assets/{assetId:D}/translations/fa",
            new UpsertMediaAssetTranslationRequest("alt"),
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

        var putFa = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/translations/fa",
            new UpsertMediaAssetTranslationRequest("نمای شهر", "کپشن", "Published"),
            ct);
        Assert.Equal(HttpStatusCode.OK, putFa.StatusCode);

        var putEnDraft = await client.PutAsJsonAsync(
            $"/api/media/assets/{assetId:D}/translations/en",
            new UpsertMediaAssetTranslationRequest("City view", PublicationStatus: "Draft"),
            ct);
        Assert.Equal(HttpStatusCode.OK, putEnDraft.StatusCode);

        var list = await client.GetAsync($"/api/media/assets/{assetId:D}/translations", ct);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync(ct));
        Assert.Equal(2, listDoc.RootElement.GetArrayLength());

        var faPresentation = await client.GetAsync(
            $"/api/media/assets/{assetId:D}/translations/fa/presentation",
            ct);
        Assert.Equal(HttpStatusCode.OK, faPresentation.StatusCode);
        using var faDoc = JsonDocument.Parse(await faPresentation.Content.ReadAsStringAsync(ct));
        Assert.Equal("نمای شهر", faDoc.RootElement.GetProperty("altText").GetString());

        var enPresentation = await client.GetAsync(
            $"/api/media/assets/{assetId:D}/translations/en/presentation",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, enPresentation.StatusCode);
    }
}
