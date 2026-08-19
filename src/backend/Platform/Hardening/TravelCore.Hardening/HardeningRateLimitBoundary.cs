namespace TravelCore.Hardening;

/// <summary>
/// P29-R2 rate limiting foundation. Abuse protection posture without middleware product or distributed store.
/// </summary>
public static class HardeningRateLimitBoundary
{
    public const string RateLimitingIsCrossCuttingPosture = "Rate limiting is cross-cutting security posture";
    public const string NoDistributedRateLimitStoreWithoutNeed =
        "No distributed rate-limit store without measured operational need";
    public const string NoWafVendorLockIn = "No WAF vendor lock-in in Hardening module";
    public const string AbuseProtectionDoesNotReplaceAuthorization =
        "Abuse protection != authorization replacement";
    public const string PublicEndpointsRequireAbusePosture =
        "Public endpoints require abuse-protection posture declaration";

    public const bool RateLimitBoundaryImplemented = true;
    public const bool RateLimiterMiddlewareImplemented = false;
    public const bool DistributedRateLimitStoreImplemented = false;
    public const bool WafIntegrationImplemented = false;
    public const bool DdosMitigationProductImplemented = false;
}
