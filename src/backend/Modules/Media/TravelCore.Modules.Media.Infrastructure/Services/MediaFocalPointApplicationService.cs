using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Persists MediaAsset focal-point metadata (SoR). No variant regeneration / crop.
/// </summary>
public sealed class MediaFocalPointApplicationService : IMediaFocalPointService
{
    private readonly MediaDbContext _db;
    private readonly IClock _clock;

    public MediaFocalPointApplicationService(MediaDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<MediaFocalPointResponse> SetAsync(
        Guid mediaAssetId,
        UpsertFocalPointRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assetId = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets
            .SingleOrDefaultAsync(x => x.Id == assetId, cancellationToken)
            ?? throw new InvalidOperationException($"MediaAsset '{mediaAssetId}' was not found.");

        var now = _clock.GetCurrentInstant();
        asset.SetFocalPoint(request.FocalX, request.FocalY, now);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(asset);
    }

    public async Task<MediaFocalPointResponse?> GetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assetId, cancellationToken);
        return asset is null ? null : Map(asset);
    }

    private static MediaFocalPointResponse Map(MediaAsset asset) =>
        new(asset.Id.Value, asset.FocalX, asset.FocalY);
}
