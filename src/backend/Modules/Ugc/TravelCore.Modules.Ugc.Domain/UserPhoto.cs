using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// UGC-owned relationship that a MediaAsset is used as user-generated content (TC-P16-T005 / P16-R5, TC-P16-T007 / P16-R7).
/// UserPhoto != MediaAsset. Logical MediaAssetId only — no technical media facts. Enters Pending directly.
/// </summary>
public sealed class UserPhoto
{
    private UserPhoto()
    {
        ModerationStatus = null!;
        PublicationStatus = null!;
    }

    private UserPhoto(UserPhotoId id, Guid actorId, Guid mediaAssetId, Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("UserPhotoId cannot be empty.", nameof(id));
        }

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("ActorId cannot be empty.", nameof(actorId));
        }

        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        Id = id;
        ActorId = actorId;
        MediaAssetId = mediaAssetId;
        var lifecycle = UgcContentLifecycle.DirectPending();
        ModerationStatus = lifecycle.ModerationStatus;
        PublicationStatus = lifecycle.PublicationStatus;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public UserPhotoId Id { get; private set; }

    /// <summary>Opaque logical actor id. Not Identity/Party ownership.</summary>
    public Guid ActorId { get; private set; }

    /// <summary>Logical Media identity only (no cross-schema FK, no StorageKey).</summary>
    public Guid MediaAssetId { get; private set; }

    public ModerationStatus ModerationStatus { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public bool IsPubliclyEligible =>
        new UgcContentLifecycle(ModerationStatus, PublicationStatus).IsPubliclyEligible;

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static UserPhoto Create(Guid actorId, Guid mediaAssetId, Instant now) =>
        new(UserPhotoId.New(), actorId, mediaAssetId, now);

    public void Approve(Instant now) => Apply(Current.Approve(), now);

    public void Reject(Instant now) => Apply(Current.Reject(), now);

    public void Publish(Instant now) => Apply(Current.Publish(), now);

    public void Hide(Instant now) => Apply(Current.Hide(), now);

    public void Archive(Instant now) => Apply(Current.Archive(), now);

    private UgcContentLifecycle Current => new(ModerationStatus, PublicationStatus);

    private void Apply(UgcContentLifecycle next, Instant now)
    {
        ModerationStatus = next.ModerationStatus;
        PublicationStatus = next.PublicationStatus;
        UpdatedAt = now;
    }
}
