namespace TravelCore.Modules.Seo.Contracts;

public sealed record SeoMetadataOverrideResponse(
    Guid Id,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string? TitleOverride,
    string? DescriptionOverride,
    DateTimeOffset UpdatedAt);

public sealed record SetSeoMetadataOverrideRequest(
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string? TitleOverride,
    string? DescriptionOverride);

/// <summary>
/// Content facts from Destination (or peer) contracts — SEO composes, does not own CMS.
/// </summary>
public sealed record ComposeSeoMetadataRequest(
    string Locale,
    string Path,
    string LocalizedTitle,
    string? LocalizedDescription);

/// <summary>
/// Server-composed page metadata for SSR consumers (TC-P05-T007).
/// Robots come from IndexPolicy evaluation (R2); canonical/hreflang reuse T004/T006.
/// </summary>
public sealed record SeoComposedMetadataResponse(
    string Locale,
    string Path,
    string Title,
    string? Description,
    bool UsedTitleOverride,
    bool UsedDescriptionOverride,
    string EffectiveIndex,
    string EffectiveFollow,
    string RobotsDirective,
    bool IsIndexable,
    IReadOnlyList<string> IndexabilityReasons,
    string? CanonicalHref,
    IReadOnlyList<SeoHreflangAlternateResponse> HreflangAlternates);

/// <summary>SEO-owned metadata composition + optional technical overrides (TC-P05-T007).</summary>
public interface ISeoMetadataService
{
    Task<SeoMetadataOverrideResponse?> GetOverrideAsync(
        string resourceType,
        Guid resourceId,
        string locale,
        CancellationToken cancellationToken = default);

    Task<SeoMetadataOverrideResponse> SetOverrideAsync(
        SetSeoMetadataOverrideRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoComposedMetadataResponse> ComposeAsync(
        ComposeSeoMetadataRequest request,
        CancellationToken cancellationToken = default);
}
