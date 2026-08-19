namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Admin moderation queue item for a travelogue (TC-MODOPS-T002).
/// Not a public composition read and not SEO indexing authority.
/// </summary>
public sealed record ModerationQueueTravelogueItem(
    Guid TravelogueId,
    Guid ActorId,
    string LocaleCode,
    string Title,
    string BodyPreview,
    string ModerationStatus,
    string PublicationStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IUgcModerationService
{
    Task<IReadOnlyList<ModerationQueueTravelogueItem>> ListPendingTraveloguesAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<ModerationQueueTravelogueItem> ApproveTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default);

    Task<ModerationQueueTravelogueItem> RejectTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default);

    Task<ModerationQueueTravelogueItem> PublishTravelogueAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default);
}
