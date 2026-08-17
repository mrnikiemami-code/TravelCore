using TravelCore.Modules.Content.Contracts;
using Xunit;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class RelatedContentPublicEligibilityTests
{
    [Fact]
    public void Public_Gate_Requires_Locale_Title_And_Slug()
    {
        Assert.True(RelatedContentPublicEligibility.IsPubliclyEligible("Guide to Yazd", "yazd-guide"));
        Assert.False(RelatedContentPublicEligibility.IsPubliclyEligible(" ", "yazd-guide"));
        Assert.False(RelatedContentPublicEligibility.IsPubliclyEligible("Guide to Yazd", " "));
        Assert.False(RelatedContentPublicEligibility.IsPubliclyEligible(null, "yazd-guide"));
        Assert.Equal(6, RelatedContentPublicEligibility.MaxItems);
    }
}
