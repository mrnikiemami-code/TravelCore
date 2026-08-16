namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// Publish Destination content-owned slug into SEO namespace as destinations/{slug} (TC-P05-T010).
/// Does not write Destination tables; does not flip IndexPolicy.
/// </summary>
public sealed record PublishDestinationSeoRouteRequest(
    Guid DestinationId,
    string Locale,
    string Slug);

public sealed record PublishDestinationSeoRouteResponse(
    SeoRouteResponse Route,
    bool Created,
    bool PathChanged,
    string PublicPath);

public interface ISeoDestinationPublicationService
{
    Task<PublishDestinationSeoRouteResponse> PublishAsync(
        PublishDestinationSeoRouteRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Publish Place content-owned slug into SEO namespace as places/{slug} (TC-P07-T007 / P07-R4).
/// Does not write Place tables; does not flip IndexPolicy (P07-R5 default remains noindex,follow).
/// </summary>
public sealed record PublishPlaceSeoRouteRequest(
    Guid PlaceId,
    string Locale,
    string Slug);

public sealed record PublishPlaceSeoRouteResponse(
    SeoRouteResponse Route,
    bool Created,
    bool PathChanged,
    string PublicPath);

public interface ISeoPlacePublicationService
{
    Task<PublishPlaceSeoRouteResponse> PublishAsync(
        PublishPlaceSeoRouteRequest request,
        CancellationToken cancellationToken = default);
}
