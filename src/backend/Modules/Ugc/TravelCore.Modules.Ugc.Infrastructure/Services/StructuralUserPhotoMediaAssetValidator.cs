using TravelCore.Modules.Ugc.Contracts;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Structural UserPhoto MediaAsset validation only (P16-R5). Non-empty logical id.
/// No Media queries or cross-schema FK.
/// </summary>
internal sealed class StructuralUserPhotoMediaAssetValidator : IUserPhotoMediaAssetValidator
{
    public void ValidateLogicalReference(Guid mediaAssetId)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }
    }
}
