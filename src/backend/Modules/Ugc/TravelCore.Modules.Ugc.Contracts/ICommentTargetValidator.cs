namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Engine-neutral port for Comment target structural validation (TC-P16-T006 / P16-R6).
/// UGC-internal only. Does not query peers or create foreign keys.
/// </summary>
public interface ICommentTargetValidator
{
    void ValidateLogicalReference(string targetType, Guid targetId);
}
