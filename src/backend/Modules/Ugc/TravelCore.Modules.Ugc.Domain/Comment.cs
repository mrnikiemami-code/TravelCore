using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Flat user-authored Comment on UGC content (TC-P16-T006 / P16-R6).
/// Targets Review or Travelogue only. No threading. Like is deferred. No publication/moderation.
/// </summary>
public sealed class Comment
{
    public const int BodyMaxLength = 8000;

    private Comment()
    {
        TargetType = null!;
        Body = null!;
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
