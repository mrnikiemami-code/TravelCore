using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Place→SeoRoute publication (TC-P07-T007). Place remains current-slug SoR (P07-R4);
/// SEO owns public path namespace, reservations, history, redirects, and IndexPolicy (P07-R5).
/// </summary>
public sealed class SeoPlacePublicationService : ISeoPlacePublicationService
{
    private readonly ISeoRouteService _routes;

    public SeoPlacePublicationService(ISeoRouteService routes)
    {
        _routes = routes;
    }

    public async Task<PublishPlaceSeoRouteResponse> PublishAsync(
        PublishPlaceSeoRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.PlaceId == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.Slug);

        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var slug = request.Slug.Trim().Trim('/');
        if (slug.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug must be a single path segment (no '/').", nameof(request));
        }

        var publicPath = SeoRoute.NormalizePath($"places/{slug}");

        var existing = await _routes.ListByResourceAsync(
            nameof(SeoResourceType.Place),
            request.PlaceId,
            cancellationToken);

        var current = existing.FirstOrDefault(r =>
            string.Equals(r.Locale, locale, StringComparison.Ordinal));

        if (current is null)
        {
            await _routes.ReservePathAsync(
                new ReserveSeoPathRequest(
                    nameof(SeoResourceType.Place),
                    request.PlaceId,
                    locale,
                    publicPath),
                cancellationToken);

            var created = await _routes.CreateAsync(
                new CreateSeoRouteRequest(
                    nameof(SeoResourceType.Place),
                    request.PlaceId,
                    locale,
                    publicPath),
                cancellationToken);

            return new PublishPlaceSeoRouteResponse(created, Created: true, PathChanged: false, publicPath);
        }

        if (string.Equals(current.Path, publicPath, StringComparison.Ordinal))
        {
            return new PublishPlaceSeoRouteResponse(current, Created: false, PathChanged: false, publicPath);
        }

        var changed = await _routes.ChangePathAsync(
            current.Id,
            new ChangeSeoRoutePathRequest(publicPath),
            cancellationToken);

        return new PublishPlaceSeoRouteResponse(
            changed.Route,
            Created: false,
            PathChanged: true,
            publicPath);
    }
}
