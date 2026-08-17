namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Engine-neutral port for Review target structural validation (TC-P16-T003 / P16-R3).
/// Does not query peer modules or create foreign keys. Existence resolution is deferred.
/// </summary>
public interface IReviewTargetValidator
{
    void ValidateLogicalReference(string targetType, Guid targetId);
}
