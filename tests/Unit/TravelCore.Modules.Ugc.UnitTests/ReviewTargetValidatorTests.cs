using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Structural Review target port (TC-P16-T003). No peer-module queries.
/// </summary>
public sealed class ReviewTargetValidatorTests
{
    [Fact]
    public void StructuralValidator_Accepts_Supported_Logical_Reference()
    {
        var validator = new StructuralReviewTargetValidator();
        validator.ValidateLogicalReference("Place", Guid.Parse("0198b3e0-0000-7000-8000-000000000041"));
        validator.ValidateLogicalReference("TourProduct", Guid.Parse("0198b3e0-0000-7000-8000-000000000042"));
        validator.ValidateLogicalReference("Agency", Guid.Parse("0198b3e0-0000-7000-8000-000000000043"));
    }

    [Fact]
    public void StructuralValidator_Rejects_Unknown_Type_Or_Empty_Id()
    {
        var validator = new StructuralReviewTargetValidator();
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Hotel", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() =>
            validator.ValidateLogicalReference("Place", Guid.Empty));
        Assert.Equal("TravelCore.Modules.Ugc.Contracts.IReviewTargetValidator", typeof(IReviewTargetValidator).FullName);
    }
}
