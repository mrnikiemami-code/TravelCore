using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Application service for SeoRoute bindings plus path history / reservation coordination.
/// SEO stores public path strings and reservations; Destination remains SoR for Translation.Slug.
/// </summary>
public sealed class SeoRouteApplicationService : ISeoRouteService
{
    private readonly SeoDbContext _db;
    private readonly IClock _clock;
    private readonly SeoRedirectApplicationService _redirects;

    public SeoRouteApplicationService(
        SeoDbContext db,
        IClock clock,
        SeoRedirectApplicationService redirects)
    {
        _db = db;
        _clock = clock;
        _redirects = redirects;
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

        var reservations = await _db.SeoPathReservations.AsNoTracking()
            .Where(x => x.Locale == locale && x.Path == path)
            .ToListAsync(cancellationToken);
        SeoPathReservation.EnsureNoForeignReservation(
            reservations,
            resourceType,
            request.ResourceId,
            locale,
            path);

        await EnsurePathNotLiveRedirectSourceAsync(locale, path, cancellationToken);

        var now = _clock.GetCurrentInstant();
        var route = SeoRoute.Create(resourceType, request.ResourceId, locale, path, now);
        _db.SeoRoutes.Add(route);

        // Consuming own pre-publish reservation when the binding is created.
        var ownReservations = await _db.SeoPathReservations
            .Where(x =>
                x.Locale == locale
                && x.Path == path
                && x.ResourceType == resourceType
                && x.ResourceId == request.ResourceId)
            .ToListAsync(cancellationToken);
        if (ownReservations.Count > 0)
        {
            _db.SeoPathReservations.RemoveRange(ownReservations);
        }

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

        return MapRoute(route);
    }

    public async Task<SeoRouteResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var routeId = SeoRouteId.From(id);
        var route = await _db.SeoRoutes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
        return route is null ? null : MapRoute(route);
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

        return routes.Select(MapRoute).ToList();
    }

    public async Task<ChangeSeoRoutePathResponse> ChangePathAsync(
        Guid seoRouteId,
        ChangeSeoRoutePathRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var routeId = SeoRouteId.From(seoRouteId);
        var route = await _db.SeoRoutes
            .FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken)
            ?? throw new KeyNotFoundException($"SeoRoute '{seoRouteId}' was not found.");

        var newPath = SeoRoute.NormalizePath(request.NewPath);
        var locale = route.Locale;

        var conflictingRoutes = await _db.SeoRoutes.AsNoTracking()
            .Where(x => x.Id != routeId && x.Locale == locale && x.Path == newPath)
            .ToListAsync(cancellationToken);
        SeoRoute.EnsureNoConflict(
            conflictingRoutes,
            route.ResourceType,
            route.ResourceId,
            locale,
            newPath,
            excludeId: routeId);

        var reservations = await _db.SeoPathReservations.AsNoTracking()
            .Where(x => x.Locale == locale && x.Path == newPath)
            .ToListAsync(cancellationToken);
        SeoPathReservation.EnsureNoForeignReservation(
            reservations,
            route.ResourceType,
            route.ResourceId,
            locale,
            newPath);

        await EnsurePathNotLiveRedirectSourceAsync(locale, newPath, cancellationToken);

        var now = _clock.GetCurrentInstant();
        var change = route.ChangePath(newPath, now);
        _db.SeoPathHistory.Add(change.History);
        _db.SeoRedirectCandidates.Add(change.RedirectCandidate);

        var ownReservations = await _db.SeoPathReservations
            .Where(x =>
                x.Locale == locale
                && x.Path == newPath
                && x.ResourceType == route.ResourceType
                && x.ResourceId == route.ResourceId)
            .ToListAsync(cancellationToken);
        if (ownReservations.Count > 0)
        {
            _db.SeoPathReservations.RemoveRange(ownReservations);
        }

        // T004: promote path-change candidate into a live chain-free permanent redirect.
        var liveRedirect = await _redirects.ActivatePermanentCoreAsync(
            route.Id,
            route.ResourceType,
            route.ResourceId,
            locale,
            change.RedirectCandidate.FromPath,
            change.RedirectCandidate.ToPath,
            change.RedirectCandidate.Id,
            now,
            cancellationToken);
        change.RedirectCandidate.MarkActivated(now);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new SeoRouteConflictException(
                "SeoRoute path change conflict: locale+path already bound or reserved.",
                ex);
        }

        return new ChangeSeoRoutePathResponse(
            MapRoute(route),
            MapHistory(change.History),
            MapCandidate(change.RedirectCandidate),
            MapRedirect(liveRedirect));
    }

    public async Task<SeoPathReservationResponse> ReservePathAsync(
        ReserveSeoPathRequest request,
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

        var activeForeignRoute = await _db.SeoRoutes.AsNoTracking()
            .AnyAsync(
                x => x.Locale == locale
                     && x.Path == path
                     && (x.ResourceType != resourceType || x.ResourceId != request.ResourceId),
                cancellationToken);
        if (activeForeignRoute)
        {
            throw new SeoRouteConflictException(
                $"Path '{path}' for locale '{locale}' is already bound to another resource.");
        }

        var reservations = await _db.SeoPathReservations.AsNoTracking()
            .Where(x => x.Locale == locale && x.Path == path)
            .ToListAsync(cancellationToken);
        SeoPathReservation.EnsureNoForeignReservation(
            reservations,
            resourceType,
            request.ResourceId,
            locale,
            path);

        await EnsurePathNotLiveRedirectSourceAsync(locale, path, cancellationToken);

        if (reservations.Any(x =>
                x.ResourceType == resourceType
                && x.ResourceId == request.ResourceId
                && string.Equals(x.Locale, locale, StringComparison.Ordinal)
                && string.Equals(x.Path, path, StringComparison.Ordinal)))
        {
            var existing = reservations[0];
            return MapReservation(existing);
        }

        var now = _clock.GetCurrentInstant();
        var reservation = SeoPathReservation.Create(
            resourceType,
            request.ResourceId,
            locale,
            path,
            now);
        _db.SeoPathReservations.Add(reservation);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new SeoRouteConflictException(
                "SeoPathReservation conflict: locale+path already reserved.",
                ex);
        }

        return MapReservation(reservation);
    }

    public async Task<bool> ReleaseReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var id = SeoPathReservationId.From(reservationId);
        var reservation = await _db.SeoPathReservations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (reservation is null)
        {
            return false;
        }

        _db.SeoPathReservations.Remove(reservation);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SeoPathHistoryResponse>> ListPathHistoryByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var type = ParseResourceType(resourceType);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var rows = await _db.SeoPathHistory.AsNoTracking()
            .Where(x => x.ResourceType == type && x.ResourceId == resourceId)
            .OrderBy(x => x.RecordedAt)
            .ThenBy(x => x.Locale)
            .ToListAsync(cancellationToken);

        return rows.Select(MapHistory).ToList();
    }

    public async Task<IReadOnlyList<SeoRedirectCandidateResponse>> ListRedirectCandidatesByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var type = ParseResourceType(resourceType);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var rows = await _db.SeoRedirectCandidates.AsNoTracking()
            .Where(x => x.ResourceType == type && x.ResourceId == resourceId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Locale)
            .ToListAsync(cancellationToken);

        return rows.Select(MapCandidate).ToList();
    }

    private async Task EnsurePathNotLiveRedirectSourceAsync(
        string locale,
        string path,
        CancellationToken cancellationToken)
    {
        var redirectSource = await _db.SeoRedirects.AsNoTracking()
            .AnyAsync(x => x.Locale == locale && x.FromPath == path, cancellationToken);
        if (redirectSource)
        {
            throw new SeoRouteConflictException(
                $"Path '{path}' for locale '{locale}' is a live redirect/gone source and cannot bind a current route.");
        }
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

    private static SeoRouteResponse MapRoute(SeoRoute route) =>
        new(
            route.Id.Value,
            route.ResourceType.ToString(),
            route.ResourceId,
            route.Locale,
            route.Path);

    private static SeoPathHistoryResponse MapHistory(SeoPathHistoryEntry entry) =>
        new(
            entry.Id.Value,
            entry.SeoRouteId.Value,
            entry.ResourceType.ToString(),
            entry.ResourceId,
            entry.Locale,
            entry.Path,
            entry.SucceededByPath,
            entry.RecordedAt.ToDateTimeOffset());

    private static SeoPathReservationResponse MapReservation(SeoPathReservation reservation) =>
        new(
            reservation.Id.Value,
            reservation.ResourceType.ToString(),
            reservation.ResourceId,
            reservation.Locale,
            reservation.Path,
            reservation.ReservedAt.ToDateTimeOffset());

    private static SeoRedirectCandidateResponse MapCandidate(SeoRedirectCandidate candidate) =>
        new(
            candidate.Id.Value,
            candidate.SeoRouteId.Value,
            candidate.ResourceType.ToString(),
            candidate.ResourceId,
            candidate.Locale,
            candidate.FromPath,
            candidate.ToPath,
            candidate.Status.ToString(),
            candidate.CreatedAt.ToDateTimeOffset());

    private static SeoRedirectResponse MapRedirect(SeoRedirect redirect) =>
        new(
            redirect.Id.Value,
            redirect.SeoRouteId?.Value,
            redirect.ResourceType.ToString(),
            redirect.ResourceId,
            redirect.Locale,
            redirect.FromPath,
            redirect.ToPath,
            redirect.Status.ToString(),
            redirect.CreatedAt.ToDateTimeOffset(),
            redirect.SourceCandidateId?.Value);
}
