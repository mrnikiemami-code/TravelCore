using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// TourProduct→SeoRoute publication (TC-P09-T008). Tour remains current-slug SoR (P09-R5);
/// SEO owns public path namespace, reservations, history, redirects, and IndexPolicy (P09-R6).
/// </summary>
public sealed class SeoTourProductPublicationService : ISeoTourProductPublicationService
{
    private readonly ISeoRouteService _routes;

    public SeoTourProductPublicationService(ISeoRouteService routes)
    {
        _routes = routes;
    }

    public async Task<PublishTourProductSeoRouteResponse> PublishAsync(
        PublishTourProductSeoRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.TourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.Slug);

        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var slug = request.Slug.Trim().Trim('/');
        if (slug.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug must be a single path segment (no '/').", nameof(request));
        }

        var publicPath = SeoRoute.NormalizePath($"tours/{slug}");

        var existing = await _routes.ListByResourceAsync(
            nameof(SeoResourceType.TourProduct),
            request.TourProductId,
            cancellationToken);

        var current = existing.FirstOrDefault(r =>
            string.Equals(r.Locale, locale, StringComparison.Ordinal));

        if (current is null)
        {
            await _routes.ReservePathAsync(
                new ReserveSeoPathRequest(
                    nameof(SeoResourceType.TourProduct),
                    request.TourProductId,
                    locale,
                    publicPath),
                cancellationToken);

            var created = await _routes.CreateAsync(
                new CreateSeoRouteRequest(
                    nameof(SeoResourceType.TourProduct),
                    request.TourProductId,
                    locale,
                    publicPath),
                cancellationToken);

            return new PublishTourProductSeoRouteResponse(created, Created: true, PathChanged: false, publicPath);
        }

        if (string.Equals(current.Path, publicPath, StringComparison.Ordinal))
        {
            return new PublishTourProductSeoRouteResponse(current, Created: false, PathChanged: false, publicPath);
        }

        var changed = await _routes.ChangePathAsync(
            current.Id,
            new ChangeSeoRoutePathRequest(publicPath),
            cancellationToken);

        return new PublishTourProductSeoRouteResponse(
            changed.Route,
            Created: false,
            PathChanged: true,
            publicPath);
    }
}
