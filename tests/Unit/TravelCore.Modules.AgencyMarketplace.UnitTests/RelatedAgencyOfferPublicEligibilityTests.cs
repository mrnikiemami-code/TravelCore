using TravelCore.Modules.AgencyMarketplace.Contracts;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

public sealed class RelatedAgencyOfferPublicEligibilityTests
{
    [Fact]
    public void Offer_Gate_Requires_Published_Listed_Active_Public_Channel()
    {
        Assert.True(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Listed",
                "Active",
                "Public"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Approved",
                "Listed",
                "Active",
                "Public"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Unlisted",
                "Active",
                "Public"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Listed",
                "Draft",
                "Public"));
        Assert.False(
            RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                "Published",
                "Listed",
                "Active",
                "AgencyPortal"));
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
