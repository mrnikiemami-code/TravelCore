using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Historical SEO public path string for a SeoRoute binding.
/// Stores path mechanics only — never Destination.Translation.Slug / name / body SoR.
/// Destination remains owner of current translation slug fields; SEO owns route-path history.
/// </summary>
public sealed class SeoPathHistoryEntry
{
    private SeoPathHistoryEntry()
    {
        Locale = null!;
        Path = null!;
        SucceededByPath = null!;
    }

    private SeoPathHistoryEntry(
        SeoPathHistoryId id,
        SeoRouteId seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        string succeededByPath,
        Instant recordedAt)
    {
        Id = id;
        SeoRouteId = seoRouteId;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        Path = path;
        SucceededByPath = succeededByPath;
        RecordedAt = recordedAt;
    }

    public SeoPathHistoryId Id { get; private set; }

    public SeoRouteId SeoRouteId { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    /// <summary>Opaque business identity — not duplicated when path history grows.</summary>
    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    /// <summary>Superseded locale-relative public path string (e.g. destinations/istanbul-city).</summary>
    public string Path { get; private set; }

    /// <summary>Path that replaced <see cref="Path"/> on the active SeoRoute.</summary>
    public string SucceededByPath { get; private set; }

    public Instant RecordedAt { get; private set; }

    public static SeoPathHistoryEntry Record(
        SeoRouteId seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string toPath,
        Instant now,
        SeoPathHistoryId? id = null)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var normalizedFrom = SeoRoute.NormalizePath(fromPath);
        var normalizedTo = SeoRoute.NormalizePath(toPath);
        if (string.Equals(normalizedFrom, normalizedTo, StringComparison.Ordinal))
        {
            throw new ArgumentException("History requires a distinct from/to path.", nameof(toPath));
        }

        return new SeoPathHistoryEntry(
            id ?? SeoPathHistoryId.New(),
            seoRouteId,
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            normalizedFrom,
            normalizedTo,
            now);
    }
}
