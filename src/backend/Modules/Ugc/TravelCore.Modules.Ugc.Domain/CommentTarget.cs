namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Polymorphic logical Comment target (P16-R6). Type + id only — no peer FK / navigation / threading.
/// </summary>
public readonly record struct CommentTarget
{
    public CommentTarget(CommentTargetType targetType, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        TargetType = targetType;
        TargetId = targetId;
    }

    public CommentTargetType TargetType { get; }

    public Guid TargetId { get; }

    public static CommentTarget Create(string targetType, Guid targetId) =>
        new(CommentTargetType.Parse(targetType), targetId);

    public override string ToString() => $"{TargetType.Value}:{TargetId:D}";
}
