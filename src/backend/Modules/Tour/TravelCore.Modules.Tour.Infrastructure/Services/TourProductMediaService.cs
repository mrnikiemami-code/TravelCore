using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// TourProduct Cover/Gallery mutations with Media.Contracts readiness validation (P09-R8).
/// </summary>
public sealed class TourProductMediaService : ITourProductMediaService
{
    private readonly TourDbContext _db;
    private readonly IMediaAssetReadinessQuery _mediaReadiness;
    private readonly IClock _clock;

    public TourProductMediaService(
        TourDbContext db,
        IMediaAssetReadinessQuery mediaReadiness,
        IClock clock)
    {
        _db = db;
        _mediaReadiness = mediaReadiness;
        _clock = clock;
    }

    public async Task<TourProductMediaResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(tourProductId, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<TourProductMediaResponse> SetCoverAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(mediaAssetId, cancellationToken);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetCover(mediaAssetId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductMediaResponse> RemoveCoverAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.RemoveCover(_clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductMediaResponse> AddGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        int? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(mediaAssetId, cancellationToken);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.AddGalleryItem(mediaAssetId, _clock.GetCurrentInstant(), sortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductMediaResponse> RemoveGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.RemoveGalleryItem(mediaAssetId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductMediaResponse> ReorderGalleryAsync(
        Guid tourProductId,
        IReadOnlyList<Guid> orderedMediaAssetIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedMediaAssetIds);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.ReorderGallery(orderedMediaAssetIds, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    private async Task EnsureReadyAsync(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        if (!await _mediaReadiness.IsReadyAsync(mediaAssetId, cancellationToken))
        {
            throw new InvalidOperationException(
                "MediaAsset must exist and be Ready to attach to a TourProduct.");
        }
    }

    private async Task<TourProduct?> FindAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.TourProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<TourProduct> LoadTrackedAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        return await FindAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"TourProduct '{tourProductId}' was not found.");
    }

    private static TourProductMediaResponse Map(TourProduct product) =>
        new(
            product.Id.Value,
            product.Code,
            product.Cover is null
                ? null
                : new TourProductMediaLinkDto(
                    product.Cover.MediaAssetId,
                    product.Cover.Role.ToString(),
                    product.Cover.SortOrder),
            product.GalleryOrdered
                .Select(x => new TourProductMediaLinkDto(x.MediaAssetId, x.Role.ToString(), x.SortOrder))
                .ToArray());
}
