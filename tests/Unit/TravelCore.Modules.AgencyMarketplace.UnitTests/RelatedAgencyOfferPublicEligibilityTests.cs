using TravelCore.Modules.AgencyMarketplace.Contracts;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

public sealed class RelatedAgencyOfferPublicEligibilityTests
{
    [Fact]
    public void Offer_Gate_Requires_Published_Listed_And_Not_Archived()
    {
        Assert.True(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Listed",
                "Active"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Approved",
                "Listed",
                "Active"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Unlisted",
                "Active"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Listed",
                "Archived"));
        Assert.Equal(6, RelatedAgencyOfferPublicEligibility.MaxItems);
    }

    [Fact]
    public void Agency_Gate_Requires_Active_And_PublicListingEnabled()
    {
        Assert.True(RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible("Active", true));
        Assert.False(RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible("Active", false));
        Assert.False(RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible("Draft", true));
        Assert.False(RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible("Archived", true));
    }
}
