using TravelCore.Modules.Tour.Contracts;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class RelatedTourPublicEligibilityTests
{
    [Fact]
    public void Only_Published_Catalog_Is_Eligible()
    {
        Assert.True(RelatedTourPublicEligibility.IsEligible("Published"));
        Assert.False(RelatedTourPublicEligibility.IsEligible("Draft"));
        Assert.False(RelatedTourPublicEligibility.IsEligible("Inactive"));
        Assert.Equal(6, RelatedTourPublicEligibility.MaxItems);
    }
}
