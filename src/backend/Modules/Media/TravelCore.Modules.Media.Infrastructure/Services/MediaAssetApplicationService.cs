using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Services;

/// <summary>
/// Application service implementing MediaAsset create/get/list (metadata SoR only).
/// </summary>
public sealed class MediaAssetApplicationService : IMediaAssetService
{
    private const int MaxListTake = 200;

    private readonly MediaDbContext _db;
    private readonly IClock _clock;

    public MediaAssetApplicationService(MediaDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<MediaAssetResponse> CreateAsync(
        CreateMediaAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var status = ParseStatus(request.Status) ?? MediaAssetStatus.PendingStorage;
        var now = _clock.GetCurrentInstant();
        var asset = MediaAsset.Create(
            request.ContentType,
            request.ByteSize,
            now,
            request.Width,
            request.Height,
            request.StorageKey,
            status);

        _db.MediaAssets.Add(asset);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "MediaAsset persistence conflict (e.g. duplicate storage_key).",
                ex);
        }

        return Map(asset);
    }

    public async Task<MediaAssetResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var assetId = MediaAssetId.From(id);
        var asset = await _db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assetId, cancellationToken);
        return asset is null ? null : Map(asset);
    }

    public async Task<IReadOnlyList<MediaAssetResponse>> ListAsync(
        string? status = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be positive.");
        }

        take = Math.Min(take, MaxListTake);
        var query = _db.MediaAssets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = ParseStatus(status)
                ?? throw new ArgumentException($"Unsupported MediaAssetStatus '{status}'.", nameof(status));
            query = query.Where(x => x.Status == parsed);
        }

        var assets = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return assets.Select(Map).ToList();
    }

    private static MediaAssetStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (Enum.TryParse<MediaAssetStatus>(status.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported MediaAssetStatus '{status}'.", nameof(status));
    }

    private static MediaAssetResponse Map(MediaAsset asset) =>
        new(
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
