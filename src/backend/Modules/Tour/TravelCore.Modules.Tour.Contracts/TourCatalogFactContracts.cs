namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Tour-owned descriptive catalog facts (TC-P09-T006) — not Booking/Payment/Pricing engines.
/// </summary>
public sealed record TourCatalogFactDto(string Code, string? Detail);

public sealed record TourProductCatalogFactsResponse(
    Guid Id,
    string Code,
    IReadOnlyList<TourCatalogFactDto> Services,
    IReadOnlyList<TourCatalogFactDto> Policies,
    IReadOnlyList<TourCatalogFactDto> Requirements);

public sealed record ReplaceTourCatalogFactsRequest(IReadOnlyList<TourCatalogFactDto> Items);

public interface ITourProductCatalogFactService
{
    Task<TourProductCatalogFactsResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<TourProductCatalogFactsResponse> ReplaceServicesAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductCatalogFactsResponse> ReplacePoliciesAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductCatalogFactsResponse> ReplaceRequirementsAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default);
}
