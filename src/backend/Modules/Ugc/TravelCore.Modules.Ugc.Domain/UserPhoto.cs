using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// UGC-owned relationship that a MediaAsset is used as user-generated content (TC-P16-T005 / P16-R5).
/// UserPhoto != MediaAsset. Logical MediaAssetId only — no technical media facts.
/// </summary>
public sealed class UserPhoto
{
    private UserPhoto()
    {
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
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public UserPhotoId Id { get; private set; }

    /// <summary>Opaque logical actor id. Not Identity/Party ownership.</summary>
    public Guid ActorId { get; private set; }

    /// <summary>Logical Media identity only (no cross-schema FK, no StorageKey).</summary>
    public Guid MediaAssetId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static UserPhoto Create(Guid actorId, Guid mediaAssetId, Instant now) =>
        new(UserPhotoId.New(), actorId, mediaAssetId, now);
}
