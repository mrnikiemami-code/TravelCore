using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class MediaUploadAccessAuthorizationTests
{
    private static readonly byte[] Png1x1 =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
        0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
        0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D,
        0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
        0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    private readonly IdentityAuthHostFixture _fixture;

    public MediaUploadAccessAuthorizationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Media_upload_enforces_authn_authz_and_yields_Ready_or_rejects_SVG()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "media-upload-authz@travelcore.test";
        const string password = "Media-Upload-Authz-Password-1";

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
            Assert.Contains(accessDb.Permissions.AsEnumerable(), x => x.Code == "media.assets.write");
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using (var anonymousContent = CreatePngContent())
        {
            var anonymous = await client.PostAsync("/api/media/assets/upload", anonymousContent, ct);
            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        }

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

        using (var deniedContent = CreatePngContent())
        {
            var authenticatedDenied = await client.PostAsync("/api/media/assets/upload", deniedContent, ct);
            Assert.Equal(HttpStatusCode.Forbidden, authenticatedDenied.StatusCode);
        }

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

        using (var allowedContent = CreatePngContent())
        {
            var allowed = await client.PostAsync("/api/media/assets/upload", allowedContent, ct);
            Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
            using var doc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync(ct));
            Assert.Equal("Ready", doc.RootElement.GetProperty("status").GetString());
            Assert.Equal("image/png", doc.RootElement.GetProperty("contentType").GetString());
            Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("storageKey").GetString()));
        }

        using (var svgAsPng = CreateSvgDisguisedAsPngContent())
        {
            var rejected = await client.PostAsync("/api/media/assets/upload", svgAsPng, ct);
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        using (var svgNamed = CreateSvgNamedContent())
        {
            var rejectedName = await client.PostAsync("/api/media/assets/upload", svgNamed, ct);
            Assert.Equal(HttpStatusCode.BadRequest, rejectedName.StatusCode);
        }
    }

    private static MultipartFormDataContent CreatePngContent()
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Png1x1);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "sample.png");
        return content;
    }

    private static MultipartFormDataContent CreateSvgDisguisedAsPngContent()
    {
        var svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>");
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(svg);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "evil.png");
        return content;
    }

    private static MultipartFormDataContent CreateSvgNamedContent()
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Png1x1);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "logo.svg");
        return content;
    }
}
