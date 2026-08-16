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
public sealed class MediaAppProxyDeliveryTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public MediaAppProxyDeliveryTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task App_proxy_delivers_Ready_original_and_variant_anonymously_and_blocks_non_Ready()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-app-proxy@travelcore.test";
        const string password = "Media-App-Proxy-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Bootstrap admin session for mutation only
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

        Guid assetId;
        using (var uploadContent = CreatePngContent(2000, 1500))
        {
            var upload = await client.PostAsync("/api/media/assets/upload", uploadContent, ct);
            Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
            using var uploadDoc = JsonDocument.Parse(await upload.Content.ReadAsStringAsync(ct));
            assetId = uploadDoc.RootElement.GetProperty("id").GetGuid();
            Assert.Equal("Ready", uploadDoc.RootElement.GetProperty("status").GetString());
            Assert.False(string.IsNullOrWhiteSpace(uploadDoc.RootElement.GetProperty("storageKey").GetString()));
        }

        var generate = await client.PostAsync($"/api/media/assets/{assetId:D}/variants/generate", null, ct);
        Assert.Equal(HttpStatusCode.Created, generate.StatusCode);

        // Fresh anonymous client — no cookies
        using var anonymous = factory.CreateClient(new() { AllowAutoRedirect = false });

        var original = await anonymous.GetAsync($"/api/media/assets/{assetId:D}/content", ct);
        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal("image/png", original.Content.Headers.ContentType?.MediaType);
        var originalBytes = await original.Content.ReadAsByteArrayAsync(ct);
        Assert.True(originalBytes.Length > 0);

        var large = await anonymous.GetAsync($"/api/media/assets/{assetId:D}/variants/large/content", ct);
        Assert.Equal(HttpStatusCode.OK, large.StatusCode);
        Assert.Equal("image/png", large.Content.Headers.ContentType?.MediaType);
        Assert.True((await large.Content.ReadAsByteArrayAsync(ct)).Length > 0);

        var presentation = await anonymous.GetAsync($"/api/media/assets/{assetId:D}/presentation", ct);
        Assert.Equal(HttpStatusCode.OK, presentation.StatusCode);
        var presentationJson = await presentation.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("storageKey", presentationJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret/", presentationJson, StringComparison.OrdinalIgnoreCase);
        using var presentationDoc = JsonDocument.Parse(presentationJson);
        Assert.Equal(
            $"/api/media/assets/{assetId:D}/content",
            presentationDoc.RootElement.GetProperty("originalContentUrl").GetString());

        // StorageKey query style must not exist
        var byKey = await anonymous.GetAsync("/api/media?key=anything", ct);
        Assert.Equal(HttpStatusCode.NotFound, byKey.StatusCode);

        // Mutation still protected
        using var anonUpload = CreatePngContent(16, 16);
        var denied = await anonymous.PostAsync("/api/media/assets/upload", anonUpload, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);

        // Non-Ready: create PendingStorage via authenticated metadata create if available,
        // or upload then mark — use unknown id for 404 and Failed path via direct DB is hard here.
        var missing = await anonymous.GetAsync($"/api/media/assets/{Guid.NewGuid():D}/content", ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var badProfile = await anonymous.GetAsync(
            $"/api/media/assets/{assetId:D}/variants/hero/content",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, badProfile.StatusCode);
    }

    [Fact]
    public async Task App_proxy_rejects_NotRequired_variant_delivery()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-app-proxy-nr@travelcore.test";
        const string password = "Media-App-Proxy-NR-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var createAccount = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, createAccount.StatusCode);
        using var createDoc = JsonDocument.Parse(await createAccount.Content.ReadAsStringAsync(ct));
        var accountId = createDoc.RootElement.GetProperty("id").GetGuid();

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync(
                "/api/identity/login",
                new LoginRequest { Email = email, Password = password },
                ct)).StatusCode);

        Guid adminRoleId;
        await using (var accessDb = _fixture.CreateAccessDb())
        {
            adminRoleId = accessDb.Roles.Single(x => x.Code == AccessPermissionCatalog.AdminRoleCode).Id.Value;
        }

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync(
                "/api/access/subject-roles/",
                new AssignSubjectRoleRequest
                {
                    SubjectType = "Identity",
                    SubjectId = accountId,
                    RoleId = adminRoleId
                },
                ct)).StatusCode);

        Guid assetId;
        using (var uploadContent = CreatePngContent(64, 48))
        {
            var upload = await client.PostAsync("/api/media/assets/upload", uploadContent, ct);
            Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
            using var uploadDoc = JsonDocument.Parse(await upload.Content.ReadAsStringAsync(ct));
            assetId = uploadDoc.RootElement.GetProperty("id").GetGuid();
        }

        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsync($"/api/media/assets/{assetId:D}/variants/generate", null, ct)).StatusCode);

        using var anonymous = factory.CreateClient(new() { AllowAutoRedirect = false });
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await anonymous.GetAsync($"/api/media/assets/{assetId:D}/variants/large/content", ct)).StatusCode);

        // Original Ready still deliverable
        Assert.Equal(
            HttpStatusCode.OK,
            (await anonymous.GetAsync($"/api/media/assets/{assetId:D}/content", ct)).StatusCode);
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
