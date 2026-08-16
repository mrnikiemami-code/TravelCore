using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Media-owned Ready probe used by Place media relationship attach (TC-P07-T005).
/// </summary>
public sealed class MediaAssetReadinessQuery : IMediaAssetReadinessQuery
{
    private readonly MediaDbContext _db;

    public MediaAssetReadinessQuery(MediaDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsReadyAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        if (mediaAssetId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        var id = MediaAssetId.From(mediaAssetId);
        return _db.MediaAssets.AsNoTracking()
            .AnyAsync(
                x => x.Id == id && x.Status == MediaAssetStatus.Ready,
                cancellationToken);
    }
}
