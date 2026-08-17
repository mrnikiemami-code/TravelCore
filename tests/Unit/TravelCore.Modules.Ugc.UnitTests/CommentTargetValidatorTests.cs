using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Structural Comment target port (TC-P16-T006). Review/Travelogue only.
/// </summary>
public sealed class CommentTargetValidatorTests
{
    [Fact]
    public void StructuralValidator_Accepts_Supported_Logical_Reference()
    {
        var validator = new StructuralCommentTargetValidator();
        validator.ValidateLogicalReference("Review", Guid.Parse("0198b3e0-0000-7000-8000-000000000083"));
        validator.ValidateLogicalReference("Travelogue", Guid.Parse("0198b3e0-0000-7000-8000-000000000084"));
    }

    [Fact]
    public void StructuralValidator_Rejects_Unknown_Type_Or_Empty_Id()
    {
        var validator = new StructuralCommentTargetValidator();
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Place", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Review", Guid.Empty));
        Assert.Equal(
            "TravelCore.Modules.Ugc.Contracts.ICommentTargetValidator",
            typeof(ICommentTargetValidator).FullName);
    }
}
