namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Polymorphic logical Review target (P16-R3). Type + id only — no peer FK / navigation.
/// </summary>
public readonly record struct ReviewTarget
{
    public ReviewTarget(ReviewTargetType targetType, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        TargetType = targetType;
        TargetId = targetId;
    }

    public ReviewTargetType TargetType { get; }

    public Guid TargetId { get; }

    public static ReviewTarget Create(string targetType, Guid targetId) =>
        new(ReviewTargetType.Parse(targetType), targetId);

    public override string ToString() => $"{TargetType.Value}:{TargetId:D}";
}
