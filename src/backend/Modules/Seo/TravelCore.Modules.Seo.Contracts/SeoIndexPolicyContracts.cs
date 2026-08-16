namespace TravelCore.Modules.Seo.Contracts;

/// <summary>Configured SEO IndexPolicy DTO (never Destination-owned).</summary>
public sealed record SeoIndexPolicyResponse(
    Guid Id,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string IndexDirective,
    string FollowDirective,
    DateTimeOffset UpdatedAt);

public sealed record SetSeoIndexPolicyRequest(
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string IndexDirective,
    string FollowDirective);

/// <summary>
/// Effective robots/indexability for frontend metadata integration (TC-P05-T005).
/// Consumers should honor RobotsDirective; IsIndexable is derived, not a Destination field.
/// </summary>
public sealed record SeoIndexabilityResponse(
    string Locale,
    string Path,
    string EffectiveIndex,
    string EffectiveFollow,
    string RobotsDirective,
    string? ConfiguredIndex,
    string? ConfiguredFollow,
    bool IsIndexable,
    IReadOnlyList<string> Reasons);

/// <summary>SEO-owned IndexPolicy + eligibility evaluation (TC-P05-T005 / R2).</summary>
public interface ISeoIndexPolicyService
{
    Task<SeoIndexPolicyResponse?> GetAsync(
        string resourceType,
        Guid resourceId,
        string locale,
        CancellationToken cancellationToken = default);

    Task<SeoIndexPolicyResponse> SetAsync(
        SetSeoIndexPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoIndexabilityResponse> EvaluatePathAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default);
}
