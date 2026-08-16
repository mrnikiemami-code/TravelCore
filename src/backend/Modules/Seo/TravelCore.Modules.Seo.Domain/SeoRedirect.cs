using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Live SEO redirect/gone record for a locale-relative public path.
/// Owns redirect mechanics only — never Destination.Translation.Slug / content SoR.
/// </summary>
public sealed class SeoRedirect
{
    public const int MaxResolutionHops = 32;

    private SeoRedirect()
    {
        Locale = null!;
        FromPath = null!;
    }

    private SeoRedirect(
        SeoRedirectId id,
        SeoRouteId? seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string? toPath,
        SeoRedirectStatus status,
        Instant createdAt,
        SeoRedirectCandidateId? sourceCandidateId)
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
        SourceCandidateId = sourceCandidateId;
    }

    public SeoRedirectId Id { get; private set; }

    public SeoRouteId? SeoRouteId { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    public string FromPath { get; private set; }

    /// <summary>Final target path for permanent redirects; null when status is Gone.</summary>
    public string? ToPath { get; private set; }

    public SeoRedirectStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public SeoRedirectCandidateId? SourceCandidateId { get; private set; }

    public static SeoRedirect CreatePermanent(
        SeoRouteId? seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string toPath,
        Instant now,
        SeoRedirectCandidateId? sourceCandidateId = null,
        SeoRedirectId? id = null)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedFrom = SeoRoute.NormalizePath(fromPath);
        var normalizedTo = SeoRoute.NormalizePath(toPath);
        if (string.Equals(normalizedFrom, normalizedTo, StringComparison.Ordinal))
        {
            throw new SeoRedirectException("Permanent redirect cannot target its own path (self-redirect).");
        }

        return new SeoRedirect(
            id ?? SeoRedirectId.New(),
            seoRouteId,
            resourceType,
            resourceId,
            normalizedLocale,
            normalizedFrom,
            normalizedTo,
            SeoRedirectStatus.PermanentMoved,
            now,
            sourceCandidateId);
    }

    public static SeoRedirect CreateGone(
        SeoRouteId? seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        Instant now,
        SeoRedirectCandidateId? sourceCandidateId = null,
        SeoRedirectId? id = null)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return new SeoRedirect(
            id ?? SeoRedirectId.New(),
            seoRouteId,
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            SeoRoute.NormalizePath(fromPath),
            toPath: null,
            SeoRedirectStatus.Gone,
            now,
            sourceCandidateId);
    }

    /// <summary>Retargets a permanent redirect to a new final path (chain flattening).</summary>
    public void RetargetPermanent(string finalToPath, Instant now)
    {
        if (Status != SeoRedirectStatus.PermanentMoved)
        {
            throw new SeoRedirectException("Only permanent redirects can be retargeted.");
        }

        var normalizedTo = SeoRoute.NormalizePath(finalToPath);
        if (string.Equals(FromPath, normalizedTo, StringComparison.Ordinal))
        {
            throw new SeoRedirectException("Permanent redirect cannot target its own path (self-redirect).");
        }

        ToPath = normalizedTo;
        CreatedAt = now;
    }

    /// <summary>Converts a permanent redirect into an intentionally gone path (no replacement).</summary>
    public void ConvertToGone(Instant now)
    {
        Status = SeoRedirectStatus.Gone;
        ToPath = null;
        CreatedAt = now;
    }
}

/// <summary>HTTP-facing redirect postures owned by T004: 301 and 410 only.</summary>
public enum SeoRedirectStatus : short
{
    PermanentMoved = 1,
    Gone = 2,
}

public sealed class SeoRedirectException : Exception
{
    public SeoRedirectException(string message)
        : base(message)
    {
    }

    public SeoRedirectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
