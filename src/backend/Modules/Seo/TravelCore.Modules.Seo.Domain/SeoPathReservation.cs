using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Reserves a public locale path string in the SEO namespace before / while binding a SeoRoute.
/// Keyed by ResourceType+ResourceId+Locale+path — not a Destination.Translation content row.
/// </summary>
public sealed class SeoPathReservation
{
    private SeoPathReservation()
    {
        Locale = null!;
        Path = null!;
    }

    private SeoPathReservation(
        SeoPathReservationId id,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        Instant reservedAt)
    {
        Id = id;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        Path = path;
        ReservedAt = reservedAt;
    }

    public SeoPathReservationId Id { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    public string Path { get; private set; }

    public Instant ReservedAt { get; private set; }

    public static SeoPathReservation Create(
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        Instant now,
        SeoPathReservationId? id = null)
    {
        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, "Unsupported SeoResourceType.");
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return new SeoPathReservation(
            id ?? SeoPathReservationId.New(),
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            SeoRoute.NormalizePath(path),
            now);
    }

    /// <summary>
    /// Active reservations block other resources from claiming the same locale+path.
    /// Same resource may hold the reservation (e.g. pre-publish hold).
    /// </summary>
    public static void EnsureNoForeignReservation(
        IEnumerable<SeoPathReservation> existingReservations,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path)
    {
        ArgumentNullException.ThrowIfNull(existingReservations);

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedPath = SeoRoute.NormalizePath(path);

        foreach (var existing in existingReservations)
        {
            if (!string.Equals(existing.Locale, normalizedLocale, StringComparison.Ordinal)
                || !string.Equals(existing.Path, normalizedPath, StringComparison.Ordinal))
            {
                continue;
            }

            if (existing.ResourceType != resourceType || existing.ResourceId != resourceId)
            {
                throw new SeoRouteConflictException(
                    $"Path '{normalizedPath}' for locale '{normalizedLocale}' is reserved by another resource.");
            }
        }
    }
}
