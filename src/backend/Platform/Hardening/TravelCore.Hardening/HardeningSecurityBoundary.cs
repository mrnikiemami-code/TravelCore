namespace TravelCore.Hardening;

/// <summary>
/// P29-R1 security foundation. Authorization review posture without identity provider product or permission engine rewrite.
/// </summary>
public static class HardeningSecurityBoundary
{
    public const string SecurityFromDayOneMandatory = "Security from day one is mandatory";
    public const string DomainOwnsAuthorizationFacts = "Domain modules own authorization facts";
    public const string PlatformOwnsCrossCuttingSecurityPosture = "Platform owns cross-cutting security posture contracts";
    public const string NoIdentityProviderLockIn = "No identity provider lock-in in Hardening module";
    public const string NoPermissionEngineRewrite = "No permission engine rewrite in early P29 tasks";
    public const string SecretsNeverInBusinessTables = "Secrets never persist in business tables";

    public const bool SecurityBoundaryImplemented = true;
    public const bool IdentityProviderProductImplemented = false;
    public const bool OauthOidcProductImplemented = false;
    public const bool PermissionEngineRewriteImplemented = false;
    public const bool AuthorizationReviewAutomationImplemented = false;
}
