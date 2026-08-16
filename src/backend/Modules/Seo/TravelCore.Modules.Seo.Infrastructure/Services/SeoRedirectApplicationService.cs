using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Canonical selection + live redirect/gone resolution (TC-P05-T004).
/// </summary>
public sealed class SeoRedirectApplicationService : ISeoRedirectService
{
    private readonly SeoDbContext _db;
    private readonly IClock _clock;

    public SeoRedirectApplicationService(SeoDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<SeoPathResolutionResponse> ResolvePathAsync(
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

        try
        {
            var resolution = SeoRedirectEngine.Resolve(normalizedLocale, normalizedPath, routes, redirects);
            return MapResolution(resolution);
        }
        catch (SeoRedirectException)
        {
            // Corrupted/looping persistence must fail closed as NotFound, not infinite redirect.
            return MapResolution(SeoPathResolution.Missing(normalizedLocale, normalizedPath));
        }
    }

    public async Task<SeoCanonicalResponse?> GetCanonicalAsync(
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

        try
        {
            var canonical = SeoRedirectEngine.SelectCanonical(
                normalizedLocale,
                normalizedPath,
                routes,
                redirects);
            return canonical is null ? null : MapCanonical(canonical);
        }
        catch (SeoRedirectException)
        {
            return null;
        }
    }

    public async Task<SeoRedirectResponse> ActivateRedirectCandidateAsync(
        ActivateSeoRedirectCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidateId = SeoRedirectCandidateId.From(request.CandidateId);
        var candidate = await _db.SeoRedirectCandidates
            .FirstOrDefaultAsync(x => x.Id == candidateId, cancellationToken)
            ?? throw new KeyNotFoundException($"SeoRedirectCandidate '{request.CandidateId}' was not found.");

        if (candidate.Status != SeoRedirectCandidateStatus.Pending)
        {
            throw new SeoRedirectException(
                $"SeoRedirectCandidate '{request.CandidateId}' is not Pending (status={candidate.Status}).");
        }

        var now = _clock.GetCurrentInstant();
        var redirect = await ActivatePermanentCoreAsync(
            candidate.SeoRouteId,
            candidate.ResourceType,
            candidate.ResourceId,
            candidate.Locale,
            candidate.FromPath,
            candidate.ToPath,
            candidate.Id,
            now,
            cancellationToken);

        candidate.MarkActivated(now);
        await _db.SaveChangesAsync(cancellationToken);
        return MapRedirect(redirect);
    }

    public async Task<SeoRedirectResponse> MarkGoneAsync(
        MarkSeoPathGoneRequest request,
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

        var activeRoute = await _db.SeoRoutes.AsNoTracking()
            .AnyAsync(x => x.Locale == locale && x.Path == path, cancellationToken);
        if (activeRoute)
        {
            throw new SeoRedirectException(
                "Active current SeoRoute path cannot be marked gone; change or retire the route first.");
        }

        var now = _clock.GetCurrentInstant();
        SeoRouteId? routeId = request.SeoRouteId is null
            ? null
            : SeoRouteId.From(request.SeoRouteId.Value);

        var existing = await _db.SeoRedirects
            .Where(x => x.Locale == locale && x.FromPath == path)
            .ToListAsync(cancellationToken);

        SeoRedirect gone;
        if (existing.Count == 0)
        {
            gone = SeoRedirect.CreateGone(
                routeId,
                resourceType,
                request.ResourceId,
                locale,
                path,
                now);
            _db.SeoRedirects.Add(gone);
        }
        else
        {
            gone = existing[0];
            gone.ConvertToGone(now);
            if (existing.Count > 1)
            {
                _db.SeoRedirects.RemoveRange(existing.Skip(1));
            }
        }

        // Any permanent redirect that targeted this path loses its replacement → Gone.
        var dependents = await _db.SeoRedirects
            .Where(x =>
                x.Locale == locale
                && x.Status == SeoRedirectStatus.PermanentMoved
                && x.ToPath == path)
            .ToListAsync(cancellationToken);
        foreach (var dependent in dependents)
        {
            dependent.ConvertToGone(now);
        }

        await EnsureHistoricalReservationAsync(
            resourceType,
            request.ResourceId,
            locale,
            path,
            now,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return MapRedirect(gone);
    }

    public async Task<IReadOnlyList<SeoRedirectResponse>> ListRedirectsByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var type = ParseResourceType(resourceType);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        var rows = await _db.SeoRedirects.AsNoTracking()
            .Where(x => x.ResourceType == type && x.ResourceId == resourceId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Locale)
            .ToListAsync(cancellationToken);

        return rows.Select(MapRedirect).ToList();
    }

    /// <summary>
    /// Used by SeoRoute path changes to promote Pending candidates into live permanent redirects.
    /// </summary>
    internal async Task<SeoRedirect> ActivatePermanentCoreAsync(
        SeoRouteId? seoRouteId,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string fromPath,
        string toPath,
        SeoRedirectCandidateId? sourceCandidateId,
        Instant now,
        CancellationToken cancellationToken)
    {
        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedFrom = SeoRoute.NormalizePath(fromPath);
        var normalizedTo = SeoRoute.NormalizePath(toPath);

        var routes = await _db.SeoRoutes.AsNoTracking()
            .Where(x => x.Locale == normalizedLocale)
            .ToListAsync(cancellationToken);
        var existingRedirects = await _db.SeoRedirects
            .Where(x => x.Locale == normalizedLocale)
            .ToListAsync(cancellationToken);

        var finalTo = SeoRedirectEngine.ComputePermanentTarget(
            normalizedLocale,
            normalizedFrom,
            normalizedTo,
            routes,
            existingRedirects,
            movingRouteId: seoRouteId);

        SeoRedirectEngine.FlattenRedirectGraph(
            normalizedLocale,
            normalizedFrom,
            finalTo,
            existingRedirects,
            now);

        var prior = existingRedirects
            .Where(x => string.Equals(x.FromPath, normalizedFrom, StringComparison.Ordinal))
            .ToList();
        foreach (var old in prior)
        {
            _db.SeoRedirects.Remove(old);
        }

        var redirect = SeoRedirect.CreatePermanent(
            seoRouteId,
            resourceType,
            resourceId,
            normalizedLocale,
            normalizedFrom,
            finalTo,
            now,
            sourceCandidateId);
        _db.SeoRedirects.Add(redirect);

        await EnsureHistoricalReservationAsync(
            resourceType,
            resourceId,
            normalizedLocale,
            normalizedFrom,
            now,
            cancellationToken);

        return redirect;
    }

    private async Task EnsureHistoricalReservationAsync(
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string path,
        Instant now,
        CancellationToken cancellationToken)
    {
        var exists = await _db.SeoPathReservations.AsNoTracking()
            .AnyAsync(
                x => x.Locale == locale
                     && x.Path == path
                     && x.ResourceType == resourceType
                     && x.ResourceId == resourceId,
                cancellationToken);
        if (exists)
        {
            return;
        }

        var foreign = await _db.SeoPathReservations.AsNoTracking()
            .Where(x => x.Locale == locale && x.Path == path)
            .ToListAsync(cancellationToken);
        SeoPathReservation.EnsureNoForeignReservation(
            foreign,
            resourceType,
            resourceId,
            locale,
            path);

        _db.SeoPathReservations.Add(
            SeoPathReservation.Create(resourceType, resourceId, locale, path, now));
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

    private static SeoPathResolutionResponse MapResolution(SeoPathResolution resolution)
    {
        var status = resolution.Kind switch
        {
            SeoPathResolutionKind.CurrentRoute => 200,
            SeoPathResolutionKind.PermanentRedirect => 301,
            SeoPathResolutionKind.Gone => 410,
            SeoPathResolutionKind.NotFound => 404,
            _ => (int?)null
        };

        return new SeoPathResolutionResponse(
            resolution.Kind.ToString(),
            resolution.Locale,
            resolution.RequestedPath,
            resolution.TargetPath,
            resolution.ResourceType?.ToString(),
            resolution.ResourceId,
            resolution.SeoRouteId,
            status);
    }

    private static SeoCanonicalResponse MapCanonical(SeoCanonicalSelection canonical) =>
        new(
            canonical.Locale,
            canonical.Path,
            canonical.ResourceType.ToString(),
            canonical.ResourceId,
            canonical.SeoRouteId,
            canonical.IsSelfCanonical);

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
