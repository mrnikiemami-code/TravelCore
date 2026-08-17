namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Engine-neutral port for UserPhoto MediaAsset structural validation (TC-P16-T005 / P16-R5).
/// Does not query Media or create foreign keys. Existence resolution is deferred.
/// </summary>
public interface IUserPhotoMediaAssetValidator
{
    void ValidateLogicalReference(Guid mediaAssetId);
}
