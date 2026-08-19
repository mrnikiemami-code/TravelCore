namespace TravelCore.Hardening;

/// <summary>
/// P29 file security vs P06 Media delivery separation. Media owns upload/delivery; Hardening declares file-security posture only.
/// </summary>
public static class HardeningMediaFileSecurityInteractionBoundary
{
    public const string MediaOwnsUploadAndDelivery = "Media owns upload validation and delivery";
    public const string HardeningDoesNotReplaceMediaDelivery = "Hardening != Media delivery replacement";
    public const string MalwareAvScanningDeferred = "Malware/AV scanning DEFERRED (P06-R7)";
    public const string FileSecurityPostureWithoutScannerProduct =
        "File security posture declared without scanner product";
    public const string StorageKeyNeverPublic = "StorageKey never public (P06 boundary preserved)";

    public const bool MediaFileSecurityInteractionBoundaryImplemented = true;
    public const bool MediaModuleReferenceRequired = false;
    public const bool MalwareScannerProductImplemented = false;
    public const bool MediaDeliveryRewriteImplemented = false;
}
