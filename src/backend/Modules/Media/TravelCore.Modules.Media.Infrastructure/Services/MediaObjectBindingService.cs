using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure.Storage;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Puts binary content via Media storage port and binds opaque StorageKey on MediaAsset.
/// </summary>
public sealed class MediaObjectBindingService : IMediaObjectBindingService
{
    private readonly MediaDbContext _db;
    private readonly IMediaObjectStorage _storage;
    private readonly IClock _clock;

    public MediaObjectBindingService(
        MediaDbContext db,
        IMediaObjectStorage storage,
        IClock clock)
    {
        _db = db;
        _storage = storage;
        _clock = clock;
    }

    public async Task<MediaAssetResponse> PutAndBindAsync(
        Guid mediaAssetId,
        Stream content,
        string contentType,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var id = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"MediaAsset '{mediaAssetId}' was not found.");

        if (!string.IsNullOrWhiteSpace(asset.StorageKey))
        {
            throw new InvalidOperationException("MediaAsset already has a StorageKey binding.");
        }

        var key = MediaStorageKeyGenerator.NewObjectKey(contentType);
        await _storage.PutAsync(
            new MediaObjectPutRequest(key, content, contentType, contentLength),
            cancellationToken);

        var now = _clock.GetCurrentInstant();
        asset.BindStorageKey(key, now, MediaAssetStatus.Ready);
        await _db.SaveChangesAsync(cancellationToken);

        return new MediaAssetResponse(
            asset.Id.Value,
            asset.ContentType,
            asset.ByteSize,
            asset.Width,
            asset.Height,
            asset.StorageKey,
            asset.Status.ToString(),
            asset.CreatedAt.ToString(),
            asset.UpdatedAt.ToString());
    }
}
