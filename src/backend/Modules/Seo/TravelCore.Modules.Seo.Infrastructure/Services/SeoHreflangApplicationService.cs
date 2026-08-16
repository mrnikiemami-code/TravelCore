using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Hreflang/alternate bindings from active SeoRoute rows only (TC-P05-T006).
/// </summary>
public sealed class SeoHreflangApplicationService : ISeoHreflangService
{
    private readonly SeoDbContext _db;

    public SeoHreflangApplicationService(SeoDbContext db)
    {
        _db = db;
    }

    public async Task<SeoHreflangBindingsResponse?> GetByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var type = ParseResourceType(resourceType);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var routes = await _db.SeoRoutes.AsNoTracking()
            .Where(x => x.ResourceType == type && x.ResourceId == resourceId)
            .ToListAsync(cancellationToken);

        if (routes.Count == 0)
        {
            return null;
        }

        var alternates = SeoHreflangEngine.BuildAlternates(type, resourceId, routes);
        return new SeoHreflangBindingsResponse(
            type.ToString(),
            resourceId,
            alternates.Select(Map).ToList());
    }

    public async Task<SeoHreflangBindingsResponse?> GetByPathAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default)
    {
        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedPath = SeoRoute.NormalizePath(path);

        var current = await _db.SeoRoutes.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Locale == normalizedLocale && x.Path == normalizedPath,
                cancellationToken);

        if (current is null)
        {
            // Do not invent alternates for historical/redirect/unknown paths.
            return null;
        }

        return await GetByResourceAsync(
            current.ResourceType.ToString(),
            current.ResourceId,
            cancellationToken);
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

    private static SeoHreflangAlternateResponse Map(SeoHreflangAlternate alternate) =>
        new(alternate.Locale, alternate.Path, alternate.Href);
}
