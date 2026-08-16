using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Application service implementing SeoRoute create/get/list-by-resource with conflict detection.
/// </summary>
public sealed class SeoRouteApplicationService : ISeoRouteService
{
    private readonly SeoDbContext _db;
    private readonly IClock _clock;

    public SeoRouteApplicationService(SeoDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<SeoRouteResponse> CreateAsync(
        CreateSeoRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var resourceType = ParseResourceType(request.ResourceType);
        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var path = SeoRoute.NormalizePath(request.Path);

        if (request.ResourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(request));
        }

        var existing = await _db.SeoRoutes.AsNoTracking()
            .Where(x =>
                (x.Locale == locale && x.Path == path)
                || (x.ResourceType == resourceType && x.ResourceId == request.ResourceId && x.Locale == locale))
            .ToListAsync(cancellationToken);

        SeoRoute.EnsureNoConflict(existing, resourceType, request.ResourceId, locale, path);

        var now = _clock.GetCurrentInstant();
        var route = SeoRoute.Create(resourceType, request.ResourceId, locale, path, now);
        _db.SeoRoutes.Add(route);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Unique indexes are the last line of defense under concurrency.
            throw new SeoRouteConflictException(
                "SeoRoute conflict: locale+path or resource+locale binding already exists.",
                ex);
        }

        return Map(route);
    }

    public async Task<SeoRouteResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var routeId = SeoRouteId.From(id);
        var route = await _db.SeoRoutes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
        return route is null ? null : Map(route);
    }

    public async Task<IReadOnlyList<SeoRouteResponse>> ListByResourceAsync(
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
            .OrderBy(x => x.Locale)
            .ThenBy(x => x.Path)
            .ToListAsync(cancellationToken);

        return routes.Select(Map).ToList();
    }

    private static SeoResourceType ParseResourceType(string resourceType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        if (Enum.TryParse<SeoResourceType>(resourceType.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Unsupported SeoResourceType '{resourceType}'.",
            nameof(resourceType));
    }

    private static SeoRouteResponse Map(SeoRoute route) =>
        new(
            route.Id.Value,
            route.ResourceType.ToString(),
            route.ResourceId,
            route.Locale,
            route.Path);
}
