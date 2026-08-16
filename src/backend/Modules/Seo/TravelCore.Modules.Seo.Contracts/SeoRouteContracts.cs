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

/// <summary>
/// Cross-module contract for SeoRoute create/get/list-by-resource (TC-P05-T002 baseline).
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
}
