namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Semantic classification + Origin/Destination links for TourProduct (TC-P09-T004 / P09-R2).
/// Destination refs are logical Guids validated via Destination.Contracts — never cross-schema FK.
/// </summary>
public sealed record TourProductSemanticLinksResponse(
    Guid Id,
    string Code,
    string? ClassificationCode,
    Guid? OriginDestinationId,
    IReadOnlyList<Guid> DestinationIds);

public sealed record SetTourClassificationRequest(string? ClassificationCode);

public sealed record SetTourOriginRequest(Guid? OriginDestinationId);

public interface ITourProductSemanticLinkService
{
    Task<TourProductSemanticLinksResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<TourProductSemanticLinksResponse> SetClassificationAsync(
        Guid tourProductId,
        SetTourClassificationRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductSemanticLinksResponse> SetOriginAsync(
        Guid tourProductId,
        SetTourOriginRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductSemanticLinksResponse> AssignDestinationAsync(
        Guid tourProductId,
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<TourProductSemanticLinksResponse> RemoveDestinationAsync(
        Guid tourProductId,
        Guid destinationId,
        CancellationToken cancellationToken = default);
}
