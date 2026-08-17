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

/// <summary>
/// Publish Article content-owned slug into SEO namespace as articles/{slug} (TC-P08-T008 / P08-R3).
/// Does not write Content tables; does not flip IndexPolicy (P08-R4 default remains noindex,follow).
/// </summary>
public sealed record PublishArticleSeoRouteRequest(
    Guid ContentItemId,
    string Locale,
    string Slug);

public sealed record PublishArticleSeoRouteResponse(
    SeoRouteResponse Route,
    bool Created,
    bool PathChanged,
    string PublicPath);

public interface ISeoArticlePublicationService
{
    Task<PublishArticleSeoRouteResponse> PublishAsync(
        PublishArticleSeoRouteRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Publish LandingPage content-owned slug into SEO namespace as landing-pages/{slug} (TC-P08-T008 / P08-R3).
/// Does not write Content tables; does not flip IndexPolicy (P08-R4 default remains noindex,follow).
/// </summary>
public sealed record PublishLandingPageSeoRouteRequest(
    Guid ContentItemId,
    string Locale,
    string Slug);

public sealed record PublishLandingPageSeoRouteResponse(
    SeoRouteResponse Route,
    bool Created,
    bool PathChanged,
    string PublicPath);

public interface ISeoLandingPagePublicationService
{
    Task<PublishLandingPageSeoRouteResponse> PublishAsync(
        PublishLandingPageSeoRouteRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Publish TourProduct translation-owned slug into SEO namespace as tours/{slug} (TC-P09-T008 / P09-R5).
/// Does not write Tour tables; does not flip IndexPolicy (P09-R6 default remains noindex,follow).
/// </summary>
public sealed record PublishTourProductSeoRouteRequest(
    Guid TourProductId,
    string Locale,
    string Slug);

public sealed record PublishTourProductSeoRouteResponse(
    SeoRouteResponse Route,
    bool Created,
    bool PathChanged,
    string PublicPath);

public interface ISeoTourProductPublicationService
{
    Task<PublishTourProductSeoRouteResponse> PublishAsync(
        PublishTourProductSeoRouteRequest request,
        CancellationToken cancellationToken = default);
}
