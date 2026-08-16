using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Binds a public locale-relative path to a business resource identity.
/// Owns route mechanics only — never Destination/Tour/Place title, body, or content slug SoR.
/// </summary>
public sealed class SeoRoute
{
    public const int LocaleMaxLength = 16;
    public const int PathMaxLength = 512;

    private SeoRoute()
    {
        Locale = null!;
        Path = null!;
    }

    private SeoRoute(
        SeoRouteId id,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        Instant createdAt)
    {
        Id = id;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        Path = path;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public SeoRouteId Id { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    /// <summary>Opaque identity owned by the business module (e.g. DestinationId.Value).</summary>
    public Guid ResourceId { get; private set; }

    /// <summary>Locale code for this public binding (e.g. fa, en). Not a content translation record.</summary>
    public string Locale { get; private set; }

    /// <summary>
    /// Locale-relative public path (no leading locale segment), e.g. destinations/istanbul.
    /// Does not store Destination display names or translation bodies.
    /// </summary>
    public string Path { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static SeoRoute Create(
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        Instant now,
        SeoRouteId? id = null)
    {
        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, "Unsupported SeoResourceType.");
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return new SeoRoute(
            id ?? SeoRouteId.New(),
            resourceType,
            resourceId,
            NormalizeLocale(locale),
            NormalizePath(path),
            now);
    }

    /// <summary>
    /// Baseline conflict rules for the public route namespace:
    /// 1) same locale+path cannot bind two different resources;
    /// 2) same resource+locale cannot hold two different active paths.
    /// </summary>
    public static void EnsureNoConflict(
        IEnumerable<SeoRoute> existingRoutes,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        SeoRouteId? excludeId = null)
    {
        ArgumentNullException.ThrowIfNull(existingRoutes);

        var normalizedLocale = NormalizeLocale(locale);
        var normalizedPath = NormalizePath(path);

        foreach (var existing in existingRoutes)
        {
            if (excludeId is not null && existing.Id == excludeId.Value)
            {
                continue;
            }

            if (string.Equals(existing.Locale, normalizedLocale, StringComparison.Ordinal)
                && string.Equals(existing.Path, normalizedPath, StringComparison.Ordinal)
                && (existing.ResourceType != resourceType || existing.ResourceId != resourceId))
            {
                throw new SeoRouteConflictException(
                    $"Path '{normalizedPath}' for locale '{normalizedLocale}' is already bound to another resource.");
            }

            if (existing.ResourceType == resourceType
                && existing.ResourceId == resourceId
                && string.Equals(existing.Locale, normalizedLocale, StringComparison.Ordinal)
                && !string.Equals(existing.Path, normalizedPath, StringComparison.Ordinal))
            {
                throw new SeoRouteConflictException(
                    $"Resource '{resourceType}:{resourceId}' already has an active path for locale '{normalizedLocale}'.");
            }
        }
    }

    public static string NormalizeLocale(string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var trimmed = locale.Trim();
        if (trimmed.Length > LocaleMaxLength)
        {
            throw new ArgumentException($"Locale max length is {LocaleMaxLength}.", nameof(locale));
        }

        // Preserve BCP-47 casing shape: language lower, region upper when present (fa, en-US).
        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    public static string NormalizePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var trimmed = path.Trim().Replace('\\', '/');

        // Locale prefix lives outside Path; strip accidental leading/trailing slashes.
        while (trimmed.StartsWith('/'))
        {
            trimmed = trimmed[1..];
        }

        while (trimmed.EndsWith('/'))
        {
            trimmed = trimmed[..^1];
        }

        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Path cannot be empty after normalization.", nameof(path));
        }

        if (trimmed.Length > PathMaxLength)
        {
            throw new ArgumentException($"Path max length is {PathMaxLength}.", nameof(path));
        }

        var segments = trimmed.Split('/', StringSplitOptions.None);
        if (segments.Any(static s => s.Length == 0 || s is "." or ".."))
        {
            throw new ArgumentException("Path segments must be non-empty and must not be '.' or '..'.", nameof(path));
        }

        // Reject whitespace-only segments and control characters; Unicode path segments remain allowed.
        if (segments.Any(static s => s.Any(char.IsControl) || s.Any(char.IsWhiteSpace)))
        {
            throw new ArgumentException("Path segments must not contain whitespace or control characters.", nameof(path));
        }

        return string.Join('/', segments);
    }
}
