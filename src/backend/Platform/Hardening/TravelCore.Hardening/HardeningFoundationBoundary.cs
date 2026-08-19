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
    public const bool AuditBoundaryImplemented = true;
    public const bool FileSecurityBoundaryImplemented = true;
    public const bool BackupDrBoundaryImplemented = true;
    public const bool OperationalPlatformBoundaryImplemented = true;
    public const bool RateLimiterImplemented = false;
    public const bool AuditEventStoreImplemented = false;
    public const bool SecretManagerImplemented = false;
    public const bool BackupAutomationImplemented = false;
}
