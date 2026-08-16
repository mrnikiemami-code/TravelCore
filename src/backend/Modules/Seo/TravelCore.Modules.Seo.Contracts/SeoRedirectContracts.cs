namespace TravelCore.Modules.Seo.Contracts;

/// <summary>Public DTO for a live SEO redirect/gone record.</summary>
public sealed record SeoRedirectResponse(
    Guid Id,
    Guid? SeoRouteId,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string FromPath,
    string? ToPath,
    string Status,
    DateTimeOffset CreatedAt,
    Guid? SourceCandidateId);

public sealed record ActivateSeoRedirectCandidateRequest(Guid CandidateId);

public sealed record MarkSeoPathGoneRequest(
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path,
    Guid? SeoRouteId);

/// <summary>
/// Outcome of public SEO path resolution (TC-P05-T004).
/// Kind: CurrentRoute | PermanentRedirect | Gone | NotFound
/// </summary>
public sealed record SeoPathResolutionResponse(
    string Kind,
    string Locale,
    string RequestedPath,
    string? TargetPath,
    string? ResourceType,
    Guid? ResourceId,
    Guid? SeoRouteId,
    int? SuggestedStatusCode);

public sealed record SeoCanonicalResponse(
    string Locale,
    string Path,
    string ResourceType,
    Guid ResourceId,
    Guid SeoRouteId,
    bool IsSelfCanonical);

/// <summary>
/// Canonical selection + redirect resolution engine contracts (TC-P05-T004).
/// </summary>
public interface ISeoRedirectService
{
    Task<SeoPathResolutionResponse> ResolvePathAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default);

    Task<SeoCanonicalResponse?> GetCanonicalAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default);

    Task<SeoRedirectResponse> ActivateRedirectCandidateAsync(
        ActivateSeoRedirectCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoRedirectResponse> MarkGoneAsync(
        MarkSeoPathGoneRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoRedirectResponse>> ListRedirectsByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);
}
