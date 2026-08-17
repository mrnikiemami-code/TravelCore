using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Structural UserPhoto MediaAsset port (TC-P16-T005). No Media queries.
/// </summary>
public sealed class UserPhotoMediaAssetValidatorTests
{
    [Fact]
    public void StructuralValidator_Accepts_NonEmpty_Logical_Id()
    {
        var validator = new StructuralUserPhotoMediaAssetValidator();
        validator.ValidateLogicalReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000073"));
    }

    [Fact]
    public void StructuralValidator_Rejects_Empty_Id()
    {
        var validator = new StructuralUserPhotoMediaAssetValidator();
        Assert.Throws<ArgumentException>(() => validator.ValidateLogicalReference(Guid.Empty));
        Assert.Equal(
            "TravelCore.Modules.Ugc.Contracts.IUserPhotoMediaAssetValidator",
            typeof(IUserPhotoMediaAssetValidator).FullName);
    }
}
