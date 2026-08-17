using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Structural UgcReport target port (TC-P16-T007). Review/Travelogue/UserPhoto/Comment only.
/// </summary>
public sealed class UgcReportTargetValidatorTests
{
    [Fact]
    public void StructuralValidator_Accepts_Supported_Logical_Reference()
    {
        var validator = new StructuralUgcReportTargetValidator();
        validator.ValidateLogicalReference("Review", Guid.Parse("0198b3e0-0000-7000-8000-0000000000b1"));
        validator.ValidateLogicalReference("Travelogue", Guid.Parse("0198b3e0-0000-7000-8000-0000000000b2"));
        validator.ValidateLogicalReference("UserPhoto", Guid.Parse("0198b3e0-0000-7000-8000-0000000000b3"));
        validator.ValidateLogicalReference("Comment", Guid.Parse("0198b3e0-0000-7000-8000-0000000000b4"));
    }

    [Fact]
    public void StructuralValidator_Rejects_Unknown_Type_Or_Empty_Id()
    {
        var validator = new StructuralUgcReportTargetValidator();
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Place", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Review", Guid.Empty));
        Assert.Equal(
            "TravelCore.Modules.Ugc.Contracts.IUgcReportTargetValidator",
            typeof(IUgcReportTargetValidator).FullName);
    }
}
