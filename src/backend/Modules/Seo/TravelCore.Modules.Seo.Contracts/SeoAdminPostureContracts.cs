namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// Job-oriented Destination SEO posture snapshot for Admin (TC-P05-T011).
/// Aggregates route publication + configured IndexPolicy + effective evaluation.
/// Does not expose Destination content fields.
/// </summary>
public sealed record SeoDestinationPostureResponse(
    Guid DestinationId,
    string Locale,
    IReadOnlyList<SeoRouteResponse> Routes,
    SeoIndexPolicyResponse? ConfiguredPolicy,
    SeoIndexabilityResponse? EffectiveIndexability,
    string Notes);

public interface ISeoAdminDestinationPostureService
{
    Task<SeoDestinationPostureResponse> GetDestinationPostureAsync(
        Guid destinationId,
        string locale,
        CancellationToken cancellationToken = default);
}
