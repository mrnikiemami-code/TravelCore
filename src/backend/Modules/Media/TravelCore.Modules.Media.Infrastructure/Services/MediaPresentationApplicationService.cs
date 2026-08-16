using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Builds app-proxy presentation DTOs for frontend MediaImage consumption (TC-P06-T009).
/// </summary>
public sealed class MediaPresentationApplicationService : IMediaPresentationService
{
    private readonly MediaDbContext _db;
    private readonly IMediaAssetTranslationService _translations;

    public MediaPresentationApplicationService(
        MediaDbContext db,
        IMediaAssetTranslationService translations)
    {
        _db = db;
        _translations = translations;
    }

    public async Task<MediaAssetPresentationResponse?> GetPresentationAsync(
        Guid mediaAssetId,
        string? localeCode = null,
        CancellationToken cancellationToken = default)
    {
        var id = MediaAssetId.From(mediaAssetId);
        var asset = await _db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (asset is null)
        {
            return null;
        }

        var status = asset.Status.ToString();
        string? originalUrl = null;
        if (asset.Status == MediaAssetStatus.Ready && !string.IsNullOrWhiteSpace(asset.StorageKey))
        {
            originalUrl = MediaPresentationUrls.OriginalContent(mediaAssetId);
        }

        var variants = await _db.MediaVariants.AsNoTracking()
            .Where(x => x.MediaAssetId == id)
            .OrderBy(x => x.Profile)
            .ToListAsync(cancellationToken);

        var variantItems = variants.Select(v =>
        {
            string? contentUrl = null;
            if (v.Status == MediaVariantStatus.Ready && !string.IsNullOrWhiteSpace(v.StorageKey))
            {
                contentUrl = MediaPresentationUrls.VariantContent(
                    mediaAssetId,
                    v.Profile.ToString());
            }

            return new MediaVariantPresentationItem(
                v.Profile.ToString(),
                v.Status.ToString(),
                contentUrl,
                v.Width,
                v.Height,
                v.ContentType);
        }).ToList();

        MediaAssetAltCaptionPresentation? altCaption = null;
        if (!string.IsNullOrWhiteSpace(localeCode))
        {
            altCaption = await _translations.GetPublishedForPresentationAsync(
                mediaAssetId,
                localeCode,
                cancellationToken);
        }

        return new MediaAssetPresentationResponse(
            mediaAssetId,
            status,
            originalUrl,
            asset.Width,
            asset.Height,
            asset.FocalX,
            asset.FocalY,
            asset.ContentType,
            variantItems,
            altCaption);
    }
}
