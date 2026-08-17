using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Experience-facing media API (TC-P10-T007 / P10-R4).
/// Guards TourKind.Experience then delegates to TourProduct Cover/Gallery (P09-R8) —
/// no second media table (would dual-source Cover). Day/Stop roles deferred.
/// </summary>
public sealed class ExperienceMediaService : IExperienceMediaService
{
    private readonly TourDbContext _db;
    private readonly ITourProductMediaService _tourMedia;

    public ExperienceMediaService(TourDbContext db, ITourProductMediaService tourMedia)
    {
        _db = db;
        _tourMedia = tourMedia;
    }

    public async Task<TourProductMediaResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        if (!await IsExperienceAsync(tourProductId, cancellationToken))
        {
            return null;
        }

        return await _tourMedia.GetAsync(tourProductId, cancellationToken);
    }

    public async Task<TourMediaPresentationResponse?> GetMediaPresentationAsync(
        Guid tourProductId,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.GetMediaPresentationAsync(tourProductId, locale, cancellationToken);
    }

    public async Task<TourProductMediaResponse> SetCoverAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.SetCoverAsync(tourProductId, mediaAssetId, cancellationToken);
    }

    public async Task<TourProductMediaResponse> RemoveCoverAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.RemoveCoverAsync(tourProductId, cancellationToken);
    }

    public async Task<TourProductMediaResponse> AddGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        int? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.AddGalleryItemAsync(tourProductId, mediaAssetId, sortOrder, cancellationToken);
    }

    public async Task<TourProductMediaResponse> RemoveGalleryItemAsync(
        Guid tourProductId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.RemoveGalleryItemAsync(tourProductId, mediaAssetId, cancellationToken);
    }

    public async Task<TourProductMediaResponse> ReorderGalleryAsync(
        Guid tourProductId,
        IReadOnlyList<Guid> orderedMediaAssetIds,
        CancellationToken cancellationToken = default)
    {
        await EnsureExperienceAsync(tourProductId, cancellationToken);
        return await _tourMedia.ReorderGalleryAsync(tourProductId, orderedMediaAssetIds, cancellationToken);
    }

    private async Task EnsureExperienceAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var product = await FindAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"TourProduct '{tourProductId}' was not found.");

        if (product.Kind != TourKind.Experience)
        {
            throw new InvalidOperationException(
                $"Experience media APIs require TourKind.Experience (found '{product.Kind}').");
        }
    }

    private async Task<bool> IsExperienceAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var product = await FindAsync(tourProductId, cancellationToken);
        return product is not null && product.Kind == TourKind.Experience;
    }

    private async Task<TourProduct?> FindAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.TourProducts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
