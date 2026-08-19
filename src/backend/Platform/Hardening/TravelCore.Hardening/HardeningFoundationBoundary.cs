namespace TravelCore.Hardening;

/// <summary>
/// P29 foundation boundary markers. Platform-owned production hardening posture without premature security product.
/// </summary>
public static class HardeningFoundationBoundary
{
    public const string SecurityFromDayOne = "LOCKED";
    public const string SecretsAreNotBusinessData = "Secrets != BusinessData";
    public const string HealthIsNotRichDiagnostics = "Health != RichDiagnostics";
    public const string AuditMetadataIsNotAuditEventProduct = "AuditMetadata != AuditEventProduct";
    public const string BuildPassIsNotTaskPass = "Build PASS != Task PASS";

    public const bool SeparateHardeningFoundationImplemented = true;
    public const bool SecurityBoundaryImplemented = true;
    public const bool RateLimitBoundaryImplemented = true;
    public const bool AuditBoundaryImplemented = false;
    public const bool FileSecurityBoundaryImplemented = false;
    public const bool BackupDrBoundaryImplemented = false;
    public const bool OperationalPlatformBoundaryImplemented = false;
    public const bool RateLimiterImplemented = false;
    public const bool AuditEventStoreImplemented = false;
    public const bool SecretManagerImplemented = false;
    public const bool BackupAutomationImplemented = false;
}
