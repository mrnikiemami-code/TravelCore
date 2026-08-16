using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Destination→SeoRoute publication (TC-P05-T010). Destination remains slug SoR;
/// SEO owns public path namespace + conflict detection.
/// </summary>
public sealed class SeoDestinationPublicationService : ISeoDestinationPublicationService
{
    private readonly ISeoRouteService _routes;

    public SeoDestinationPublicationService(ISeoRouteService routes)
    {
        _routes = routes;
    }

    public async Task<PublishDestinationSeoRouteResponse> PublishAsync(
        PublishDestinationSeoRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.DestinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.Slug);

        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var slug = request.Slug.Trim().Trim('/');
        if (slug.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug must be a single path segment (no '/').", nameof(request));
        }

        var publicPath = SeoRoute.NormalizePath($"destinations/{slug}");

        var existing = await _routes.ListByResourceAsync(
            nameof(SeoResourceType.Destination),
            request.DestinationId,
            cancellationToken);

        var current = existing.FirstOrDefault(r =>
            string.Equals(r.Locale, locale, StringComparison.Ordinal));

        if (current is null)
        {
            // Reserve then create — own reservation is consumed by CreateAsync.
            await _routes.ReservePathAsync(
                new ReserveSeoPathRequest(
                    nameof(SeoResourceType.Destination),
                    request.DestinationId,
                    locale,
                    publicPath),
                cancellationToken);

            var created = await _routes.CreateAsync(
                new CreateSeoRouteRequest(
                    nameof(SeoResourceType.Destination),
                    request.DestinationId,
                    locale,
                    publicPath),
                cancellationToken);

            return new PublishDestinationSeoRouteResponse(created, Created: true, PathChanged: false, publicPath);
        }

        if (string.Equals(current.Path, publicPath, StringComparison.Ordinal))
        {
            return new PublishDestinationSeoRouteResponse(current, Created: false, PathChanged: false, publicPath);
        }

        var changed = await _routes.ChangePathAsync(
            current.Id,
            new ChangeSeoRoutePathRequest(publicPath),
            cancellationToken);

        return new PublishDestinationSeoRouteResponse(
            changed.Route,
            Created: false,
            PathChanged: true,
            publicPath);
    }
}
