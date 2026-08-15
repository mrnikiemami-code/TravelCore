using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TravelCore.Modules.Identity.Infrastructure.Security;

public static class IdentityCookieAuthenticationExtensions
{
    /// <summary>
    /// Registers cookie authentication for browser/Admin Identity tickets (R1).
    /// Production uses Secure=Always; Development/Testing may use SameAsRequest for local HTTP probes.
    /// </summary>
    public static IServiceCollection AddTravelCoreIdentityCookieAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddAuthentication(IdentityCookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(IdentityCookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = IdentityCookieAuthenticationDefaults.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.IsEssential = true;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.Events.OnRedirectToLogin = static context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = static context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddOptions<CookieAuthenticationOptions>(IdentityCookieAuthenticationDefaults.AuthenticationScheme)
            .PostConfigure<IHostEnvironment>((options, environment) =>
            {
                // Dev-only difference: SameAsRequest allows TestServer/HTTP probes.
                // Production/Staging always require Secure cookies.
                options.Cookie.SecurePolicy = environment.IsProduction() || environment.IsStaging()
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
            });

        services.AddAuthorization();
        return services;
    }
}
