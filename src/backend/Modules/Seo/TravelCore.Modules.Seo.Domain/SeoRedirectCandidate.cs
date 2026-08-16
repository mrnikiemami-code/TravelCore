using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Hook recorded when an SEO-bound path changes — input for the future Redirect engine (T004).
/// Not a live HTTP redirect and not Destination content ownership.
/// </summary>
public sealed class SeoRedirectCandidate
{
    private SeoRedirectCandidate()
    {
        Locale = null!;
        FromPath = null!;
        ToPath = null!;
    }

    private SeoRedirectCandidate(
        SeoRedirectCandidateId id,
        SeoRouteId seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string toPath,
        SeoRedirectCandidateStatus status,
        Instant createdAt)
    {
        Id = id;
        SeoRouteId = seoRouteId;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        FromPath = fromPath;
        ToPath = toPath;
        Status = status;
        CreatedAt = createdAt;
    }

    public SeoRedirectCandidateId Id { get; private set; }

    public SeoRouteId SeoRouteId { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    public string FromPath { get; private set; }

    public string ToPath { get; private set; }

    public SeoRedirectCandidateStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static SeoRedirectCandidate CreatePending(
        SeoRouteId seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string toPath,
        Instant now,
        SeoRedirectCandidateId? id = null)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var normalizedFrom = SeoRoute.NormalizePath(fromPath);
        var normalizedTo = SeoRoute.NormalizePath(toPath);
        if (string.Equals(normalizedFrom, normalizedTo, StringComparison.Ordinal))
        {
            throw new ArgumentException("Redirect candidate requires distinct from/to paths.", nameof(toPath));
        }

        return new SeoRedirectCandidate(
            id ?? SeoRedirectCandidateId.New(),
            seoRouteId,
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            normalizedFrom,
            normalizedTo,
            SeoRedirectCandidateStatus.Pending,
            now);
    }
}

/// <summary>
/// Lifecycle of a redirect candidate before T004 promotes it to a Redirect record.
/// </summary>
public enum SeoRedirectCandidateStatus : short
{
    Pending = 1,
}
