using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Metadata composition + optional technical overrides (TC-P05-T007).
/// Robots from IndexPolicy; canonical/hreflang reused — Destination remains content SoR.
/// </summary>
public sealed class SeoMetadataApplicationService : ISeoMetadataService
{
    private readonly SeoDbContext _db;
    private readonly IClock _clock;
    private readonly ISeoIndexPolicyService _indexPolicies;
    private readonly ISeoRedirectService _redirects;
    private readonly ISeoHreflangService _hreflang;

    public SeoMetadataApplicationService(
        SeoDbContext db,
        IClock clock,
        ISeoIndexPolicyService indexPolicies,
        ISeoRedirectService redirects,
        ISeoHreflangService hreflang)
    {
        _db = db;
        _clock = clock;
        _indexPolicies = indexPolicies;
        _redirects = redirects;
        _hreflang = hreflang;
    }

    public async Task<SeoMetadataOverrideResponse?> GetOverrideAsync(
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

        var row = await _db.SeoMetadataOverrides.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ResourceType == type
                     && x.ResourceId == resourceId
                     && x.Locale == normalizedLocale,
                cancellationToken);

        return row is null ? null : MapOverride(row);
    }

    public async Task<SeoMetadataOverrideResponse> SetOverrideAsync(
        SetSeoMetadataOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var type = ParseResourceType(request.ResourceType);
        var locale = SeoRoute.NormalizeLocale(request.Locale);
        if (request.ResourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(request));
        }

        var now = _clock.GetCurrentInstant();
        var existing = await _db.SeoMetadataOverrides
            .FirstOrDefaultAsync(
                x => x.ResourceType == type
                     && x.ResourceId == request.ResourceId
                     && x.Locale == locale,
                cancellationToken);

        if (existing is null)
        {
            existing = SeoMetadataOverride.Create(
                type,
                request.ResourceId,
                locale,
                request.TitleOverride,
                request.DescriptionOverride,
                now);
            _db.SeoMetadataOverrides.Add(existing);
        }
        else
        {
            existing.Replace(request.TitleOverride, request.DescriptionOverride, now);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return MapOverride(existing);
    }

    public async Task<SeoComposedMetadataResponse> ComposeAsync(
        ComposeSeoMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LocalizedTitle);

        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var path = SeoRoute.NormalizePath(request.Path);

        var indexability = await _indexPolicies.EvaluatePathAsync(locale, path, cancellationToken);
        var canonical = await _redirects.GetCanonicalAsync(locale, path, cancellationToken);
        var hreflang = await _hreflang.GetByPathAsync(locale, path, cancellationToken);

        SeoMetadataOverrideValues? overrideValues = null;
        if (canonical is not null)
        {
            var row = await _db.SeoMetadataOverrides.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ResourceType == Enum.Parse<SeoResourceType>(canonical.ResourceType, ignoreCase: true)
                         && x.ResourceId == canonical.ResourceId
                         && x.Locale == locale,
                    cancellationToken);

            if (row is not null)
            {
                overrideValues = new SeoMetadataOverrideValues(row.TitleOverride, row.DescriptionOverride);
            }
        }

        var composed = SeoMetadataCompositionEngine.Compose(
            new SeoMetadataContentInput(request.LocalizedTitle, request.LocalizedDescription),
            overrideValues);

        string? canonicalHref = canonical is null
            ? null
            : $"/{canonical.Locale}/{canonical.Path}";

        return new SeoComposedMetadataResponse(
            locale,
            path,
            composed.Title,
            composed.Description,
            composed.UsedTitleOverride,
            composed.UsedDescriptionOverride,
            indexability.EffectiveIndex,
            indexability.EffectiveFollow,
            indexability.RobotsDirective,
            indexability.IsIndexable,
            indexability.Reasons,
            canonicalHref,
            hreflang?.Alternates ?? Array.Empty<SeoHreflangAlternateResponse>());
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

    private static SeoMetadataOverrideResponse MapOverride(SeoMetadataOverride row) =>
        new(
            row.Id.Value,
            row.ResourceType.ToString(),
            row.ResourceId,
            row.Locale,
            row.TitleOverride,
            row.DescriptionOverride,
            row.UpdatedAt.ToDateTimeOffset());
}
