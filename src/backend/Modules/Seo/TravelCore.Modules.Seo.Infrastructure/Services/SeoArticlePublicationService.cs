using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Article→SeoRoute publication (TC-P08-T008). Content remains current-slug SoR (P08-R3);
/// SEO owns public path namespace, reservations, history, redirects, and IndexPolicy (P08-R4).
/// </summary>
public sealed class SeoArticlePublicationService : ISeoArticlePublicationService
{
    private readonly ISeoRouteService _routes;

    public SeoArticlePublicationService(ISeoRouteService routes)
    {
        _routes = routes;
    }

    public async Task<PublishArticleSeoRouteResponse> PublishAsync(
        PublishArticleSeoRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ContentItemId == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.Slug);

        var locale = SeoRoute.NormalizeLocale(request.Locale);
        var slug = request.Slug.Trim().Trim('/');
        if (slug.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug must be a single path segment (no '/').", nameof(request));
        }

        var publicPath = SeoRoute.NormalizePath($"articles/{slug}");

        var existing = await _routes.ListByResourceAsync(
            nameof(SeoResourceType.Article),
            request.ContentItemId,
            cancellationToken);

        var current = existing.FirstOrDefault(r =>
            string.Equals(r.Locale, locale, StringComparison.Ordinal));

        if (current is null)
        {
            await _routes.ReservePathAsync(
                new ReserveSeoPathRequest(
                    nameof(SeoResourceType.Article),
                    request.ContentItemId,
                    locale,
                    publicPath),
                cancellationToken);

            var created = await _routes.CreateAsync(
                new CreateSeoRouteRequest(
                    nameof(SeoResourceType.Article),
                    request.ContentItemId,
                    locale,
                    publicPath),
                cancellationToken);

            return new PublishArticleSeoRouteResponse(created, Created: true, PathChanged: false, publicPath);
        }

        if (string.Equals(current.Path, publicPath, StringComparison.Ordinal))
        {
            return new PublishArticleSeoRouteResponse(current, Created: false, PathChanged: false, publicPath);
        }

        var changed = await _routes.ChangePathAsync(
            current.Id,
            new ChangeSeoRoutePathRequest(publicPath),
            cancellationToken);

        return new PublishArticleSeoRouteResponse(
            changed.Route,
            Created: false,
            PathChanged: true,
            publicPath);
    }
}
