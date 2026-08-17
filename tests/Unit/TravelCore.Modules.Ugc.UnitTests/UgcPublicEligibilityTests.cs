using TravelCore.Modules.Ugc.Contracts;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Public UGC eligibility and composition contracts (TC-P16-T008 / P16-R8).
/// </summary>
public sealed class UgcPublicEligibilityTests
{
    [Fact]
    public void Public_Eligibility_Is_Approved_Plus_Published_Only()
    {
        Assert.True(UgcPublicEligibility.IsPubliclyEligible("Approved", "Published"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Approved", "Hidden"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Approved", "Draft"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Approved", "Archived"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Pending", "Published"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Rejected", "Published"));
        Assert.False(UgcPublicEligibility.IsPubliclyEligible("Rejected", "Hidden"));
        Assert.Equal(6, UgcPublicEligibility.MaxReviews);
        Assert.Equal(6, UgcPublicEligibility.MaxTravelogues);
        Assert.Equal(6, UgcPublicEligibility.MaxUserPhotos);
        Assert.Equal(6, UgcPublicEligibility.MaxComments);
    }

    [Fact]
    public void Public_Composition_Does_Not_Own_Seo_Or_Search()
    {
        Assert.Equal("Ugc", UgcPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", UgcPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Search", UgcPublicCompositionBoundary.SearchOwner);
        Assert.Equal("Seo", UgcPublicCompositionBoundary.IndexPolicyOwner);
        Assert.Equal("DerivedRebuildableReadModel", UgcPublicCompositionBoundary.RatingSummaryPosture);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(UgcPublicCompositionBoundary.IndependentAverageRatingEngineAllowed);
        Assert.False(UgcPublicCompositionBoundary.SearchEngineInThisTaskAllowed);
        Assert.False(UgcPublicCompositionBoundary.UgcOwnedSeoPagesAllowed);
        Assert.False(UgcPublicCompositionBoundary.CopyUgcIntoCatalogAllowed);
        Assert.True(UgcOwnershipBoundary.PublicReadContractsImplemented);
        Assert.True(UgcOwnershipBoundary.RatingSummaryIsDerivedRebuildable);
        Assert.False(UgcOwnershipBoundary.SearchEngineInUgcAllowed);
        Assert.False(UgcOwnershipBoundary.UgcOwnedSeoPagesAllowed);
        Assert.False(UgcOwnershipBoundary.IndependentAverageRatingEngineAllowed);
        Assert.False(UgcOwnershipBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(UgcOwnershipBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.NotNull(typeof(IUgcPublicReviewQuery));
        Assert.NotNull(typeof(IUgcPublicTravelogueQuery));
        Assert.NotNull(typeof(IUgcPublicUserPhotoQuery));
        Assert.NotNull(typeof(IUgcPublicCommentQuery));
        Assert.NotNull(typeof(EligiblePublicReviewPage));
        Assert.NotNull(typeof(EligiblePublicRatingSummary));
    }
}
