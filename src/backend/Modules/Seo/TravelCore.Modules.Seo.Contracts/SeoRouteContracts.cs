namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// Public DTO for a SeoRoute binding. Never exposes EF entities or Destination content fields.
/// </summary>
public sealed record SeoRouteResponse(
    Guid Id,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path);

public sealed record CreateSeoRouteRequest(
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path);

public sealed record ChangeSeoRoutePathRequest(string NewPath);

public sealed record ChangeSeoRoutePathResponse(
    SeoRouteResponse Route,
    SeoPathHistoryResponse History,
    SeoRedirectCandidateResponse RedirectCandidate,
    SeoRedirectResponse? Redirect);

public sealed record SeoPathHistoryResponse(
    Guid Id,
    Guid SeoRouteId,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path,
    string SucceededByPath,
    DateTimeOffset RecordedAt);

public sealed record ReserveSeoPathRequest(
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path);

public sealed record SeoPathReservationResponse(
    Guid Id,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string Path,
    DateTimeOffset ReservedAt);

public sealed record SeoRedirectCandidateResponse(
    Guid Id,
    Guid SeoRouteId,
    string ResourceType,
    Guid ResourceId,
    string Locale,
    string FromPath,
    string ToPath,
    string Status,
    DateTimeOffset CreatedAt);

/// <summary>
/// Cross-module contract for SeoRoute create/get/list-by-resource (TC-P05-T002)
/// plus path history / reservation / redirect-candidate coordination (TC-P05-T003).
/// </summary>
public interface ISeoRouteService
{
    Task<SeoRouteResponse> CreateAsync(
        CreateSeoRouteRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoRouteResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoRouteResponse>> ListByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the SEO-bound public path: writes history + pending redirect-candidate hook,
    /// then activates a live permanent redirect (T004).
    /// Does not mutate Destination.Translation.Slug (Destination remains content-slug SoR).
    /// </summary>
    Task<ChangeSeoRoutePathResponse> ChangePathAsync(
        Guid seoRouteId,
        ChangeSeoRoutePathRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoPathReservationResponse> ReservePathAsync(
        ReserveSeoPathRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ReleaseReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoPathHistoryResponse>> ListPathHistoryByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoRedirectCandidateResponse>> ListRedirectCandidatesByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);
}
