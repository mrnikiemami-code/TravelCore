using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// IndexPolicy persistence + eligibility evaluation against live route/redirect state (TC-P05-T005).
/// </summary>
public sealed class SeoIndexPolicyApplicationService : ISeoIndexPolicyService
{
    private readonly SeoDbContext _db;
    private readonly IClock _clock;

    public SeoIndexPolicyApplicationService(SeoDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<SeoIndexPolicyResponse?> GetAsync(
        string resourceType,
        Guid resourceId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var type = ParseResourceType(resourceType);
        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var row = await _db.SeoIndexPolicies.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ResourceType == type
                     && x.ResourceId == resourceId
                     && x.Locale == normalizedLocale,
                cancellationToken);

        return row is null ? null : MapPolicy(row);
    }

    public async Task<SeoIndexPolicyResponse> SetAsync(
        SetSeoIndexPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var type = ParseResourceType(request.ResourceType);
        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var index = ParseIndex(request.IndexDirective);
        var follow = ParseFollow(request.FollowDirective);
        if (request.ResourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(request));
        }

        var now = _clock.GetCurrentInstant();
        var existing = await _db.SeoIndexPolicies
            .FirstOrDefaultAsync(
                x => x.ResourceType == type
                     && x.ResourceId == request.ResourceId
                     && x.Locale == locale,
                cancellationToken);

        if (existing is null)
        {
            existing = SeoIndexPolicy.Create(type, request.ResourceId, locale, index, follow, now);
            _db.SeoIndexPolicies.Add(existing);
        }
        else
        {
            existing.Replace(index, follow, now);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return MapPolicy(existing);
    }

    public async Task<SeoIndexabilityResponse> EvaluatePathAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default)
    {
        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedPath = SeoRoute.NormalizePath(path);

        var routes = await _db.SeoRoutes.AsNoTracking()
            .Where(x => x.Locale == normalizedLocale)
            .ToListAsync(cancellationToken);
        var redirects = await _db.SeoRedirects.AsNoTracking()
            .Where(x => x.Locale == normalizedLocale)
            .ToListAsync(cancellationToken);

        SeoPathResolution resolution;
        SeoCanonicalSelection? canonical;
        try
        {
            resolution = SeoRedirectEngine.Resolve(normalizedLocale, normalizedPath, routes, redirects);
            canonical = SeoRedirectEngine.SelectCanonical(normalizedLocale, normalizedPath, routes, redirects);
        }
        catch (SeoRedirectException)
        {
            resolution = SeoPathResolution.Missing(normalizedLocale, normalizedPath);
            canonical = null;
        }

        SeoIndexPolicy? policy = null;
        if (resolution.ResourceType is not null && resolution.ResourceId is not null)
        {
            policy = await _db.SeoIndexPolicies.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ResourceType == resolution.ResourceType.Value
                         && x.ResourceId == resolution.ResourceId.Value
                         && x.Locale == normalizedLocale,
                    cancellationToken);
        }

        var evaluation = SeoIndexPolicyEngine.Evaluate(
            normalizedLocale,
            normalizedPath,
            policy,
            resolution,
            canonical);

        return MapEvaluation(evaluation);
    }

    private static SeoResourceType ParseResourceType(string resourceType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        if (Enum.TryParse<SeoResourceType>(resourceType.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported SeoResourceType '{resourceType}'.", nameof(resourceType));
    }

    private static SeoIndexDirective ParseIndex(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Enum.TryParse<SeoIndexDirective>(value.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported IndexDirective '{value}'.", nameof(value));
    }

    private static SeoFollowDirective ParseFollow(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Enum.TryParse<SeoFollowDirective>(value.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported FollowDirective '{value}'.", nameof(value));
    }

    private static SeoIndexPolicyResponse MapPolicy(SeoIndexPolicy policy) =>
        new(
            policy.Id.Value,
            policy.ResourceType.ToString(),
            policy.ResourceId,
            policy.Locale,
            policy.IndexDirective.ToString(),
            policy.FollowDirective.ToString(),
            policy.UpdatedAt.ToDateTimeOffset());

    private static SeoIndexabilityResponse MapEvaluation(SeoIndexabilityEvaluation evaluation) =>
        new(
            evaluation.Locale,
            evaluation.Path,
            evaluation.EffectiveIndex.ToString(),
            evaluation.EffectiveFollow.ToString(),
            evaluation.RobotsDirective,
            evaluation.ConfiguredIndex?.ToString(),
            evaluation.ConfiguredFollow?.ToString(),
            evaluation.IsIndexable,
            evaluation.Reasons);
}
