namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Experience media relations (TC-P10-T007 / P10-R4).
/// Experience is a TourProduct specialization — Cover/Gallery persist as TourProductMediaLink (P09-R8).
/// Day/Stop media roles are explicitly deferred (not invented).
/// </summary>
public interface IExperienceMediaService
{
    Task<TourProductMediaResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<TourMediaPresentationResponse?> GetMediaPresentationAsync(
        Guid tourProductId,
        string? locale = null,
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
