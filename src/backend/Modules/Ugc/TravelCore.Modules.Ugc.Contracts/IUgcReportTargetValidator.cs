namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Engine-neutral port for UgcReport target structural validation (TC-P16-T007 / P16-R7).
/// UGC-internal only. Does not query peers, hide content, or create foreign keys.
/// </summary>
public interface IUgcReportTargetValidator
{
    void ValidateLogicalReference(string targetType, Guid targetId);
}
