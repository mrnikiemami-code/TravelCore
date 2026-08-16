namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// TourProduct media relations (TC-P09-T007 / P09-R8) — Cover 0..1, Gallery 0..N.
/// </summary>
public sealed record TourProductMediaLinkDto(
    Guid MediaAssetId,
    string Role,
    int SortOrder);

public sealed record TourProductMediaResponse(
    Guid Id,
    string Code,
    TourProductMediaLinkDto? Cover,
    IReadOnlyList<TourProductMediaLinkDto> Gallery);

public interface ITourProductMediaService
{
    Task<TourProductMediaResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<TourProductMediaResponse> SetCoverAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<TourProductMediaResponse> RemoveCoverAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<TourProductMediaResponse> AddGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        int? sortOrder = null,
        CancellationToken cancellationToken = default);

    Task<TourProductMediaResponse> RemoveGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<TourProductMediaResponse> ReorderGalleryAsync(
        Guid tourProductId,
        IReadOnlyList<Guid> orderedMediaAssetIds,
        CancellationToken cancellationToken = default);
}
