using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure.Processing;
using TravelCore.Modules.Media.Infrastructure.Storage;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Synchronous Media-owned variant generation (P06-R3). Separate from upload.
/// Failed derived variants never invalidate original Ready; technical blob compensation only.
/// </summary>
public sealed class MediaVariantApplicationService : IMediaVariantProcessingService
{
    private readonly MediaDbContext _db;
    private readonly IMediaObjectStorage _storage;
    private readonly ImageSharpMediaVariantProcessor _processor;
    private readonly IClock _clock;

    public MediaVariantApplicationService(
        MediaDbContext db,
        IMediaObjectStorage storage,
        ImageSharpMediaVariantProcessor processor,
        IClock clock)
    {
        _db = db;
        _storage = storage;
        _processor = processor;
        _clock = clock;
    }

    public async Task<IReadOnlyList<MediaVariantResponse>> GenerateForAssetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets
            .SingleOrDefaultAsync(x => x.Id == assetId, cancellationToken)
            ?? throw new InvalidOperationException($"MediaAsset '{mediaAssetId}' was not found.");

        if (asset.Status != MediaAssetStatus.Ready)
        {
            throw new InvalidOperationException(
                $"MediaAsset '{mediaAssetId}' must be Ready before variant generation (status={asset.Status}).");
        }

        if (string.IsNullOrWhiteSpace(asset.StorageKey))
        {
            throw new InvalidOperationException(
                $"MediaAsset '{mediaAssetId}' has no StorageKey binding.");
        }

        ImageSharpMediaVariantProcessor.EnsureSupportedOutputFormat(asset.ContentType);

        await using var original = await _storage.OpenReadAsync(asset.StorageKey, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Original blob for MediaAsset '{mediaAssetId}' was not found in storage.");

        var committedNewKeys = new List<string>();
        var supersededKeys = new List<string>();
        try
        {
            using var decoded = await _processor.DecodeAsync(original.Content, asset.ContentType, cancellationToken);
            var now = _clock.GetCurrentInstant();
            asset.SetDimensions(decoded.Width, decoded.Height, now);

            var existing = await _db.MediaVariants
                .Where(x => x.MediaAssetId == assetId)
                .ToListAsync(cancellationToken);
            var byProfile = existing.ToDictionary(x => x.Profile);

            foreach (var profile in MediaVariantSizingPolicy.AllDerivedProfiles)
            {
                await UpsertProfileAsync(
                    asset,
                    decoded,
                    profile,
                    byProfile,
                    committedNewKeys,
                    supersededKeys,
                    cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            // Persist succeeded — new keys are owned by MediaVariant rows; do not compensate them.
            committedNewKeys.Clear();

            foreach (var obsoleteKey in supersededKeys.Distinct(StringComparer.Ordinal))
            {
                try
                {
                    await _storage.DeleteAsync(obsoleteKey, cancellationToken);
                }
                catch
                {
                    // Best-effort cleanup of superseded derived blobs (not R8 domain delete).
                }
            }

            return await ListForAssetAsync(mediaAssetId, cancellationToken);
        }
        catch
        {
            foreach (var key in committedNewKeys.Distinct(StringComparer.Ordinal))
            {
                try
                {
                    await _storage.DeleteAsync(key, cancellationToken);
                }
                catch
                {
                    // Best-effort technical compensation only.
                }
            }

            throw;
        }
    }

    public async Task<IReadOnlyList<MediaVariantResponse>> ListForAssetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var rows = await _db.MediaVariants
            .AsNoTracking()
            .Where(x => x.MediaAssetId == assetId)
            .OrderBy(x => x.Profile)
            .ToListAsync(cancellationToken);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<MediaVariantResponse?> GetByIdAsync(
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var id = MediaVariantId.From(variantId);
        var row = await _db.MediaVariants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return row is null ? null : ToResponse(row);
    }

    private async Task UpsertProfileAsync(
        MediaAsset asset,
        DecodedRasterImage decoded,
        MediaVariantProfile profile,
        Dictionary<MediaVariantProfile, MediaVariant> byProfile,
        List<string> committedNewKeys,
        List<string> supersededKeys,
        CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        byProfile.TryGetValue(profile, out var existing);
        var previousKey = existing?.TakePreviousStorageKeyForReplace();
        string? putKeyThisAttempt = null;

        try
        {
            if (MediaVariantSizingPolicy.IsNotRequired(decoded.Width, decoded.Height, profile))
            {
                if (existing is null)
                {
                    var created = MediaVariant.CreateNotRequired(
                        asset.Id,
                        profile,
                        decoded.Width,
                        decoded.Height,
                        now);
                    _db.MediaVariants.Add(created);
                    byProfile[profile] = created;
                }
                else
                {
                    existing.ReplaceAsNotRequired(decoded.Width, decoded.Height, now);
                }

                if (!string.IsNullOrWhiteSpace(previousKey))
                {
                    supersededKeys.Add(previousKey);
                }

                return;
            }

            var (targetWidth, targetHeight) = MediaVariantSizingPolicy.FitWithinProfile(
                decoded.Width,
                decoded.Height,
                profile);
            var encoded = await _processor.EncodeFitWithinAsync(
                decoded,
                targetWidth,
                targetHeight,
                cancellationToken);

            putKeyThisAttempt = MediaStorageKeyGenerator.NewVariantObjectKey(encoded.ContentType, profile);
            await using var putStream = new MemoryStream(encoded.Bytes, writable: false);
            await _storage.PutAsync(
                new MediaObjectPutRequest(
                    putKeyThisAttempt,
                    putStream,
                    encoded.ContentType,
                    encoded.Bytes.LongLength),
                cancellationToken);
            committedNewKeys.Add(putKeyThisAttempt);

            if (existing is null)
            {
                var created = MediaVariant.CreateReady(
                    asset.Id,
                    profile,
                    encoded.Width,
                    encoded.Height,
                    encoded.Bytes.LongLength,
                    putKeyThisAttempt,
                    encoded.ContentType,
                    now);
                _db.MediaVariants.Add(created);
                byProfile[profile] = created;
            }
            else
            {
                existing.ReplaceAsReady(
                    encoded.Width,
                    encoded.Height,
                    encoded.Bytes.LongLength,
                    putKeyThisAttempt,
                    encoded.ContentType,
                    now);
            }

            if (!string.IsNullOrWhiteSpace(previousKey)
                && !string.Equals(previousKey, putKeyThisAttempt, StringComparison.Ordinal))
            {
                supersededKeys.Add(previousKey);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!string.IsNullOrWhiteSpace(putKeyThisAttempt))
            {
                try
                {
                    await _storage.DeleteAsync(putKeyThisAttempt, cancellationToken);
                }
                catch
                {
                    // Best-effort compensation of this attempt's blob.
                }

                committedNewKeys.Remove(putKeyThisAttempt);
            }

            // First-time profile failure → Failed row. Existing Ready/NotRequired left unchanged on regenerate miss.
            if (existing is null)
            {
                now = _clock.GetCurrentInstant();
                var failed = MediaVariant.CreateFailed(
                    asset.Id,
                    profile,
                    now,
                    failureReason: ex.Message,
                    width: decoded.Width,
                    height: decoded.Height);
                _db.MediaVariants.Add(failed);
                byProfile[profile] = failed;
            }
        }
    }

    private static MediaVariantResponse ToResponse(MediaVariant variant) =>
        new(
            variant.Id.Value,
            variant.MediaAssetId.Value,
            variant.Profile.ToString(),
            variant.Status.ToString(),
            variant.Width,
            variant.Height,
            variant.ByteSize,
            variant.StorageKey,
            variant.ContentType,
            variant.FailureReason,
            variant.CreatedAt.ToString(),
            variant.UpdatedAt.ToString());
}
