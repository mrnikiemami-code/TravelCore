using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Infrastructure.Security;
using Xunit;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class IdentityCookieAuthenticationTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public IdentityCookieAuthenticationTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Production_cookie_options_are_httponly_samesite_lax_secure_always()
    {
        using var factory = _fixture.CreateFactory(Environments.Production);
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityCookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(IdentityCookieAuthenticationDefaults.CookieName, options.Cookie.Name);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
    }

    [Fact]
    public void Development_cookie_options_allow_same_as_request_for_local_http_probes()
    {
        using var factory = _fixture.CreateFactory(Environments.Development);
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityCookieAuthenticationDefaults.AuthenticationScheme);

        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
    }

    [Fact]
    public async Task Login_logout_me_cookie_flow_and_auth_does_not_grant_access()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "auth-host@travelcore.test";
        const string password = "Auth-Host-Password-1";

        await using (var db = _fixture.CreateIdentityDb())
        {
            var existing = db.Accounts.FirstOrDefault(x => x.NormalizedEmail == email.ToUpperInvariant());
            if (existing is not null)
            {
                db.Accounts.Remove(existing);
                await db.SaveChangesAsync(ct);
            }
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var create = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var createBody = await create.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("PasswordHash", createBody, StringComparison.OrdinalIgnoreCase);
        using var createDoc = JsonDocument.Parse(createBody);
        var createdId = createDoc.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, createdId);

        var anonymousMe = await client.GetAsync(new Uri("/api/identity/me", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousMe.StatusCode);

        var badPassword = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = "wrong-password-xx" },
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, badPassword.StatusCode);
        Assert.False(badPassword.Headers.TryGetValues("Set-Cookie", out _));

        var unknown = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = "missing@travelcore.test", Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.True(login.Headers.TryGetValues("Set-Cookie", out var setCookies));
        var setCookie = string.Join("\n", setCookies);
        Assert.Contains(IdentityCookieAuthenticationDefaults.CookieName, setCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
        var loginBody = await login.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("PasswordHash", loginBody, StringComparison.OrdinalIgnoreCase);
        using var loginDoc = JsonDocument.Parse(loginBody);
        Assert.Equal(createdId, loginDoc.RootElement.GetProperty("accountId").GetGuid());

        var me = await client.GetAsync(new Uri("/api/identity/me", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var meDoc = JsonDocument.Parse(await me.Content.ReadAsStringAsync(ct));
        Assert.Equal(createdId, meDoc.RootElement.GetProperty("accountId").GetGuid());

        await using (var accessDb = _fixture.CreateAccessDb())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(accessDb, SystemClock.Instance, ct);
        }

        var evaluate = await client.PostAsJsonAsync(
            "/api/access/evaluate",
            new EvaluateAccessRequest
            {
                SubjectType = "Identity",
                SubjectId = createdId,
                PermissionCode = "access.roles.read"
            },
            ct);
        Assert.Equal(HttpStatusCode.OK, evaluate.StatusCode);
        using (var doc = JsonDocument.Parse(await evaluate.Content.ReadAsStringAsync(ct)))
        {
            Assert.False(doc.RootElement.GetProperty("allowed").GetBoolean());
            Assert.Equal("Deny", doc.RootElement.GetProperty("decision").GetString());
        }

        var logout = await client.PostAsync(new Uri("/api/identity/logout", UriKind.Relative), null, ct);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var meAfterLogout = await client.GetAsync(new Uri("/api/identity/me", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Disabled_account_cannot_login()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "disabled-auth@travelcore.test";
        const string password = "Disabled-Auth-Password-1";

        await using (var db = _fixture.CreateIdentityDb())
        {
            var hasher = new AspNetCoreIdentityPasswordHasher();
            var account = AccountAggregate.Create(email, hasher.HashPassword(password), SystemClock.Instance.GetCurrentInstant());
            account.Disable(SystemClock.Instance.GetCurrentInstant());
            db.Accounts.Add(account);
            await db.SaveChangesAsync(ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.False(login.Headers.TryGetValues("Set-Cookie", out _));
    }
}
