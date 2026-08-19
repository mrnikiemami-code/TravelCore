namespace TravelCore.Hardening;

/// <summary>
/// P29-R4 content sanitization foundation. User-generated and editorial content hygiene posture without sanitizer product.
/// </summary>
public static class HardeningContentSanitizationBoundary
{
    public const string ContentSanitizationIsCrossCuttingPosture = "Content sanitization is cross-cutting posture";
    public const string EditorialContentOwnedByContentModule = "Editorial content owned by Content module";
    public const string UgcContentOwnedByUgcModule = "UGC content owned by UGC module";
    public const string NoHtmlSanitizerVendorLockIn = "No HTML sanitizer vendor lock-in in Hardening module";
    public const string SanitizationDoesNotReplaceModuleValidation =
        "Sanitization posture != module validation replacement";

    public const bool ContentSanitizationBoundaryImplemented = true;
    public const bool HtmlSanitizerProductImplemented = false;
    public const bool CrossModuleContentSanitizerImplemented = false;
}
