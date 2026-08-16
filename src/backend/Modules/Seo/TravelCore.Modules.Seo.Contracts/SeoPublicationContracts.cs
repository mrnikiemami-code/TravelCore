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
