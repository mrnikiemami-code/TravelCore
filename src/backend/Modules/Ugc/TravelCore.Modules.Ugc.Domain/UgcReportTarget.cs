namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Polymorphic logical UgcReport target (P16-R7). Type + id only — no FK / navigation / ownership of the target.
/// </summary>
public readonly record struct UgcReportTarget
{
    public UgcReportTarget(UgcReportTargetType targetType, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        TargetType = targetType;
        TargetId = targetId;
    }

    public UgcReportTargetType TargetType { get; }

    public Guid TargetId { get; }

    public static UgcReportTarget Create(string targetType, Guid targetId) =>
        new(UgcReportTargetType.Parse(targetType), targetId);

    public override string ToString() => $"{TargetType.Value}:{TargetId:D}";
}
