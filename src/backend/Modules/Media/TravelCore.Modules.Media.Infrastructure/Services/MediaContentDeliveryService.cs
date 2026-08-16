using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// App-proxy content opener (P06-R4). Resolves by Media identity/profile only — never by StorageKey.
/// </summary>
public sealed class MediaContentDeliveryService : IMediaContentDeliveryService
{
    private readonly MediaDbContext _db;
    private readonly IMediaObjectStorage _storage;

    public MediaContentDeliveryService(MediaDbContext db, IMediaObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<MediaContentDeliveryResult?> OpenOriginalAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var id = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (asset is null)
        {
            return null;
        }

        if (asset.Status != MediaAssetStatus.Ready
            || string.IsNullOrWhiteSpace(asset.StorageKey))
        {
            return null;
        }

        var opened = await _storage.OpenReadAsync(asset.StorageKey, cancellationToken);
        if (opened is null)
        {
            return null;
        }

        return new MediaContentDeliveryResult(
            opened.Content,
            asset.ContentType,
            opened.ContentLength,
            mediaAssetId,
            "original");
    }

    public async Task<MediaContentDeliveryResult?> OpenVariantAsync(
        Guid mediaAssetId,
        string profile,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseProfile(profile, out var parsed))
        {
            return null;
        }

        var id = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (asset is null || asset.Status != MediaAssetStatus.Ready)
        {
            return null;
        }

        var variant = await _db.MediaVariants.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MediaAssetId == id && x.Profile == parsed,
                cancellationToken);

        if (variant is null
            || variant.Status != MediaVariantStatus.Ready
            || string.IsNullOrWhiteSpace(variant.StorageKey)
            || string.IsNullOrWhiteSpace(variant.ContentType))
        {
            return null;
        }

        var opened = await _storage.OpenReadAsync(variant.StorageKey, cancellationToken);
        if (opened is null)
        {
            return null;
        }

        return new MediaContentDeliveryResult(
            opened.Content,
            variant.ContentType,
            opened.ContentLength,
            mediaAssetId,
            MediaPresentationUrls.NormalizeProfileSegment(parsed.ToString()));
    }

    internal static bool TryParseProfile(string? profile, out MediaVariantProfile parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(profile))
        {
            return false;
        }

        return Enum.TryParse(profile.Trim(), ignoreCase: true, out parsed)
               && Enum.IsDefined(parsed);
    }
}
