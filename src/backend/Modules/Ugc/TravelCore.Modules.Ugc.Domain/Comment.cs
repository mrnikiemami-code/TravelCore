using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Flat user-authored Comment on UGC content (TC-P16-T006 / P16-R6, TC-P16-T007 / P16-R7).
/// Targets Review or Travelogue only. No threading. Like is deferred. Enters Pending directly.
/// </summary>
public sealed class Comment
{
    public const int BodyMaxLength = 8000;

    private Comment()
    {
        TargetType = null!;
        Body = null!;
        ModerationStatus = null!;
        PublicationStatus = null!;
    }

    private Comment(
        CommentId id,
        Guid actorId,
        CommentTarget target,
        string body,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("CommentId cannot be empty.", nameof(id));
        }

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("ActorId cannot be empty.", nameof(actorId));
        }

        Id = id;
        ActorId = actorId;
        TargetType = target.TargetType;
        TargetId = target.TargetId;
        Body = NormalizeBody(body);
        var lifecycle = UgcContentLifecycle.DirectPending();
        ModerationStatus = lifecycle.ModerationStatus;
        PublicationStatus = lifecycle.PublicationStatus;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public CommentId Id { get; private set; }

    /// <summary>Opaque logical actor id. Not Identity/Party ownership.</summary>
    public Guid ActorId { get; private set; }

    public CommentTargetType TargetType { get; private set; }

    public Guid TargetId { get; private set; }

    public CommentTarget Target => new(TargetType, TargetId);

    public string Body { get; private set; }

    public ModerationStatus ModerationStatus { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public bool IsPubliclyEligible =>
        new UgcContentLifecycle(ModerationStatus, PublicationStatus).IsPubliclyEligible;

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static Comment Create(
        Guid actorId,
        string targetType,
        Guid targetId,
        string body,
        Instant now) =>
        new(CommentId.New(), actorId, CommentTarget.Create(targetType, targetId), body, now);

    public void SetBody(string body, Instant now)
    {
        Body = NormalizeBody(body);
        UpdatedAt = now;
    }

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

    private static string NormalizeBody(string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        var trimmed = body.Trim();
        if (trimmed.Length > BodyMaxLength)
        {
            throw new ArgumentException($"body cannot exceed {BodyMaxLength} characters.", nameof(body));
        }

        return trimmed;
    }
}
