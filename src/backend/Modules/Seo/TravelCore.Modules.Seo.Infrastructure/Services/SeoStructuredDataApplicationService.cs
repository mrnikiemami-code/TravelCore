using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>Structured-data projections (TC-P05-T008). Breadcrumb only — no fake ratings/prices.</summary>
public sealed class SeoStructuredDataApplicationService : ISeoStructuredDataService
{
    public Task<SeoBreadcrumbListResponse?> ComposeBreadcrumbAsync(
        ComposeSeoBreadcrumbRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var nodes = (request.Nodes ?? Array.Empty<SeoBreadcrumbNodeRequest>())
            .Select(n => new SeoBreadcrumbNodeInput(n.Name, n.PublicPath));

        var doc = SeoStructuredDataEngine.BuildBreadcrumbList(request.Locale, nodes);
        if (doc is null)
        {
            return Task.FromResult<SeoBreadcrumbListResponse?>(null);
        }

        var response = new SeoBreadcrumbListResponse(
            doc.Context,
            doc.Type,
            doc.ItemListElement
                .Select(i => new SeoBreadcrumbListItemResponse(i.Type, i.Position, i.Name, i.Item))
                .ToList());

        return Task.FromResult<SeoBreadcrumbListResponse?>(response);
    }
}
