using Microsoft.AspNetCore.Authentication.Cookies;

namespace TravelCore.Modules.Identity.Infrastructure.Security;

/// <summary>
/// TravelCore Identity cookie authentication defaults (R1: secure HttpOnly cookie).
/// </summary>
public static class IdentityCookieAuthenticationDefaults
{
    public const string AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    /// <summary>
    /// Explicit TravelCore-owned auth cookie name (not frontend-readable).
    /// </summary>
    public const string CookieName = "TravelCore.Identity";

    public const string AccountIdClaimType = "tc_account_id";
}
