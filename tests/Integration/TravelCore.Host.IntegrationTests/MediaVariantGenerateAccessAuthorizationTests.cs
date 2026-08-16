using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class MediaVariantGenerateAccessAuthorizationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public MediaVariantGenerateAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Media_variant_generate_enforces_authn_authz_matrix()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-variant-authz@travelcore.test";
        const string password = "Media-Variant-Authz-Password-1";
        var assetId = Guid.Parse("11111111-1111-7111-8111-111111111111");

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "media.assets.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var anonymous = await client.PostAsync($"/api/media/assets/{assetId:D}/variants/generate", null, ct);
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

        var authenticatedDenied = await client.PostAsync(
            $"/api/media/assets/{assetId:D}/variants/generate",
            null,
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

        using (var uploadContent = CreatePngContent(width: 64, height: 48))
        {
            var upload = await client.PostAsync("/api/media/assets/upload", uploadContent, ct);
            Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
            using var uploadDoc = JsonDocument.Parse(await upload.Content.ReadAsStringAsync(ct));
            assetId = uploadDoc.RootElement.GetProperty("id").GetGuid();
        }

        var allowed = await client.PostAsync($"/api/media/assets/{assetId:D}/variants/generate", null, ct);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        using var variantsDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
        Assert.Equal(JsonValueKind.Array, variantsDoc.RootElement.ValueKind);
        Assert.Equal(3, variantsDoc.RootElement.GetArrayLength());
        foreach (var item in variantsDoc.RootElement.EnumerateArray())
        {
            Assert.Equal("NotRequired", item.GetProperty("status").GetString());
        }

        var listed = await client.GetAsync($"/api/media/assets/{assetId:D}/variants", ct);
        Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
    }

    private static MultipartFormDataContent CreatePngContent(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        var bytes = ms.ToArray();

        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "sample.png");
        return content;
    }
}
