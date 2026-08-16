using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Persists MediaAsset alt/caption translation rows and exposes ADR 0008-safe presentation reads.
/// </summary>
public sealed class MediaAssetTranslationApplicationService : IMediaAssetTranslationService
{
    private readonly MediaDbContext _db;
    private readonly IClock _clock;

    public MediaAssetTranslationApplicationService(MediaDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<MediaAssetTranslationResponse> UpsertAsync(
        Guid mediaAssetId,
        string localeCode,
        UpsertMediaAssetTranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assetId = MediaAssetId.From(mediaAssetId);
        var exists = await _db.MediaAssets.AsNoTracking()
            .AnyAsync(x => x.Id == assetId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"MediaAsset '{mediaAssetId}' was not found.");
        }

        var normalizedLocale = MediaAssetTranslation.NormalizeLocaleCode(localeCode);
        var now = _clock.GetCurrentInstant();
        var status = ParsePublicationStatus(request.PublicationStatus);

        var existing = await _db.MediaAssetTranslations
            .SingleOrDefaultAsync(
                x => x.MediaAssetId == assetId && x.LocaleCode == normalizedLocale,
                cancellationToken);

        if (existing is null)
        {
            var created = MediaAssetTranslation.Create(
                assetId,
                normalizedLocale,
                request.AltText,
                now,
                request.Caption,
                status ?? MediaTranslationPublicationStatus.Draft);
            _db.MediaAssetTranslations.Add(created);
            await _db.SaveChangesAsync(cancellationToken);
            return Map(created);
        }

        existing.Update(request.AltText, request.Caption, now, status);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(existing);
    }

    public async Task<MediaAssetTranslationResponse?> GetAsync(
        Guid mediaAssetId,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var normalizedLocale = MediaAssetTranslation.NormalizeLocaleCode(localeCode);
        var row = await _db.MediaAssetTranslations.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MediaAssetId == assetId && x.LocaleCode == normalizedLocale,
                cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<MediaAssetTranslationResponse>> ListAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var rows = await _db.MediaAssetTranslations.AsNoTracking()
            .Where(x => x.MediaAssetId == assetId)
            .OrderBy(x => x.LocaleCode)
            .ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<MediaAssetAltCaptionPresentation?> GetPublishedForPresentationAsync(
        Guid mediaAssetId,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(mediaAssetId);
        var normalizedLocale = MediaAssetTranslation.NormalizeLocaleCode(localeCode);

        // Exact locale only — no silent cross-locale invent (ADR 0008).
        var row = await _db.MediaAssetTranslations.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MediaAssetId == assetId
                    && x.LocaleCode == normalizedLocale
                    && x.PublicationStatus == MediaTranslationPublicationStatus.Published,
                cancellationToken);

        return row is null
            ? null
            : new MediaAssetAltCaptionPresentation(
                row.MediaAssetId.Value,
                row.LocaleCode,
                row.AltText,
                row.Caption);
    }

    private static MediaTranslationPublicationStatus? ParsePublicationStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (Enum.TryParse<MediaTranslationPublicationStatus>(status.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Unsupported MediaTranslationPublicationStatus '{status}'.",
            nameof(status));
    }

    private static MediaAssetTranslationResponse Map(MediaAssetTranslation row) =>
        new(
            row.MediaAssetId.Value,
            row.LocaleCode,
            row.AltText,
            row.Caption,
            row.PublicationStatus.ToString(),
            row.CreatedAt.ToString(),
            row.UpdatedAt.ToString());
}
