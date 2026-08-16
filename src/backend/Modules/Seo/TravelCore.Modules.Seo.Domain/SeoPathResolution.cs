namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Deterministic public path resolution outcome for a locale + path lookup.
/// </summary>
public enum SeoPathResolutionKind : short
{
    /// <summary>Active current SeoRoute binding — serve content; no redirect.</summary>
    CurrentRoute = 1,

    /// <summary>Historical path with permanent replacement — HTTP 301.</summary>
    PermanentRedirect = 2,

    /// <summary>Intentionally retired path with no replacement — HTTP 410.</summary>
    Gone = 3,

    /// <summary>Unknown path — HTTP 404 (never soft-404 / never 410).</summary>
    NotFound = 4,
}

/// <summary>
/// Result of resolving a locale-relative public SEO path.
/// </summary>
public sealed record SeoPathResolution(
    SeoPathResolutionKind Kind,
    string Locale,
    string RequestedPath,
    string? TargetPath,
    SeoResourceType? ResourceType,
    Guid? ResourceId,
    Guid? SeoRouteId)
{
    public static SeoPathResolution Current(
        string locale,
        string path,
        SeoResourceType resourceType,
        Guid resourceId,
        Guid seoRouteId) =>
        new(
            SeoPathResolutionKind.CurrentRoute,
            locale,
            path,
            TargetPath: path,
            resourceType,
            resourceId,
            seoRouteId);

    public static SeoPathResolution Permanent(
        string locale,
        string requestedPath,
        string targetPath,
        SeoResourceType resourceType,
        Guid resourceId,
        Guid? seoRouteId) =>
        new(
            SeoPathResolutionKind.PermanentRedirect,
            locale,
            requestedPath,
            targetPath,
            resourceType,
            resourceId,
            seoRouteId);

    public static SeoPathResolution GonePath(
        string locale,
        string requestedPath,
        SeoResourceType resourceType,
        Guid resourceId,
        Guid? seoRouteId) =>
        new(
            SeoPathResolutionKind.Gone,
            locale,
            requestedPath,
            TargetPath: null,
            resourceType,
            resourceId,
            seoRouteId);

    public static SeoPathResolution Missing(string locale, string requestedPath) =>
        new(
            SeoPathResolutionKind.NotFound,
            locale,
            requestedPath,
            TargetPath: null,
            ResourceType: null,
            ResourceId: null,
            SeoRouteId: null);
}

/// <summary>
/// Self-canonical selection for an active public route (tracking params are never part of Path).
/// </summary>
public sealed record SeoCanonicalSelection(
    string Locale,
    string Path,
    SeoResourceType ResourceType,
    Guid ResourceId,
    Guid SeoRouteId,
    bool IsSelfCanonical);
