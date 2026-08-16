using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure.Storage;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Validates upload content, creates PendingStorage asset, puts blob, binds Ready.
/// On failure after put: technical DeleteAsync compensation (not P06-R8 domain delete).
/// </summary>
public sealed class MediaUploadApplicationService : IMediaUploadService
{
    private const int SniffByteCount = 512;

    private readonly MediaDbContext _db;
    private readonly IMediaObjectStorage _storage;
    private readonly IClock _clock;
    private readonly MediaUploadOptions _options;

    public MediaUploadApplicationService(
        MediaDbContext db,
        IMediaObjectStorage storage,
        IClock clock,
        IOptions<MediaUploadOptions> options)
    {
        _db = db;
        _storage = storage;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<MediaAssetResponse> UploadAsync(
        Stream content,
        string contentType,
        string? fileName = null,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        MediaUploadContentRules.ValidateFileName(fileName);
        var normalizedType = MediaUploadContentRules.NormalizeAndRequireAllowedContentType(contentType);

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var size = MediaUploadContentRules.NormalizeAndRequireSize(buffer.Length, _options.MaxBytes);
        if (contentLength is > 0 && contentLength.Value != size)
        {
            throw new ArgumentException(
                "Declared Content-Length does not match actual upload size.",
                nameof(contentLength));
        }

        buffer.Position = 0;
        var sniffLength = (int)Math.Min(SniffByteCount, buffer.Length);
        Span<byte> head = stackalloc byte[sniffLength];
        var read = buffer.Read(head);
        MediaUploadContentRules.ValidatePayload(head[..read], normalizedType);
        buffer.Position = 0;

        var now = _clock.GetCurrentInstant();
        var asset = MediaAsset.Create(normalizedType, size, now, status: MediaAssetStatus.PendingStorage);
        _db.MediaAssets.Add(asset);
        await _db.SaveChangesAsync(cancellationToken);

        string? putKey = null;
        try
        {
            putKey = MediaStorageKeyGenerator.NewObjectKey(normalizedType);
            await _storage.PutAsync(
                new MediaObjectPutRequest(putKey, buffer, normalizedType, size),
                cancellationToken);

            now = _clock.GetCurrentInstant();
            asset.BindStorageKey(putKey, now, MediaAssetStatus.Ready);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(putKey))
            {
                try
                {
                    await _storage.DeleteAsync(putKey, cancellationToken);
                }
                catch
                {
                    // Best-effort technical compensation only; do not mask original failure.
                }
            }

            now = _clock.GetCurrentInstant();
            asset.MarkFailed(now);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Preserve original exception path below.
            }

            throw;
        }

        return new MediaAssetResponse(
            asset.Id.Value,
            asset.ContentType,
            asset.ByteSize,
            asset.Width,
            asset.Height,
            asset.FocalX,
            asset.FocalY,
            asset.StorageKey,
            asset.Status.ToString(),
            asset.CreatedAt.ToString(),
            asset.UpdatedAt.ToString());
    }
}
