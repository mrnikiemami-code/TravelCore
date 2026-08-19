using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Admin UGC moderation operations (TC-MODOPS-T003).
/// Travelogue pending queue + approve/reject/publish. Not Content CMS or Search.
/// </summary>
internal sealed class UgcModerationService : IUgcModerationService
{
    private const int MaxTake = 200;
    private const int BodyPreviewLength = 240;

    private readonly UgcDbContext _db;
    private readonly IClock _clock;

    public UgcModerationService(UgcDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ModerationQueueTravelogueItem>> ListPendingTraveloguesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive.");
        }

        if (take > MaxTake)
        {
            throw new ArgumentOutOfRangeException(nameof(take), $"Take cannot exceed {MaxTake}.");
        }

        var rows = await _db.Travelogues
            .AsNoTracking()
            .Where(x =>
                x.ModerationStatus == ModerationStatus.Pending
                && x.PublicationStatus != PublicationStatus.Archived)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(MapItem).ToList();
    }

    public async Task<ModerationQueueTravelogueItem> ApproveTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default)
    {
        var travelogue = await LoadMutableTravelogueAsync(travelogueId, cancellationToken);
        travelogue.Approve(_clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return MapItem(travelogue);
    }

    public async Task<ModerationQueueTravelogueItem> RejectTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default)
    {
        var travelogue = await LoadMutableTravelogueAsync(travelogueId, cancellationToken);
        travelogue.Reject(_clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return MapItem(travelogue);
    }

    public async Task<ModerationQueueTravelogueItem> PublishTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default)
    {
        var travelogue = await LoadMutableTravelogueAsync(travelogueId, cancellationToken);
        travelogue.Publish(_clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return MapItem(travelogue);
    }

    private async Task<Travelogue> LoadMutableTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken)
    {
        if (travelogueId == Guid.Empty)
        {
            throw new ArgumentException("TravelogueId cannot be empty.", nameof(travelogueId));
        }

        var travelogue = await _db.Travelogues
            .FirstOrDefaultAsync(x => x.Id.Value == travelogueId, cancellationToken);
        if (travelogue is null)
        {
            throw new KeyNotFoundException($"Travelogue '{travelogueId:D}' was not found.");
        }

        return travelogue;
    }

    private static ModerationQueueTravelogueItem MapItem(Travelogue travelogue) =>
        new(
            travelogue.Id.Value,
            travelogue.ActorId,
            travelogue.LocaleCode,
            travelogue.Title,
            PreviewBody(travelogue.Body),
            travelogue.ModerationStatus.Value,
            travelogue.PublicationStatus.Value,
            ToUtc(travelogue.CreatedAt),
            ToUtc(travelogue.UpdatedAt));

    private static string PreviewBody(string body)
    {
        var trimmed = body.Trim();
        if (trimmed.Length <= BodyPreviewLength)
        {
            return trimmed;
        }

        return trimmed[..BodyPreviewLength] + "…";
    }

    private static DateTimeOffset ToUtc(Instant instant) => instant.ToDateTimeOffset();
}
