using TravelCore.Modules.PublicExperience.Contracts;
using Xunit;

namespace TravelCore.Modules.PublicExperience.UnitTests;

/// <summary>
/// Scaffolding smoke for Public Experience surfaces (TC-P14-T001 / P14-R1).
/// </summary>
public sealed class PublicExperienceScaffoldingSmokeTests
{
    [Fact]
    public void PublicExperienceContractsAssembly_IsLoadable()
    {
        var marker = typeof(PublicExperienceContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.PublicExperience.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.PublicExperience.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void Surfaces_Are_Detail_Listing_Landing_Only()
    {
        Assert.Equal(1, (short)PublicExperienceSurfaceKind.Detail);
        Assert.Equal(2, (short)PublicExperienceSurfaceKind.Listing);
        Assert.Equal(3, (short)PublicExperienceSurfaceKind.Landing);
        Assert.Equal(3, Enum.GetValues<PublicExperienceSurfaceKind>().Length);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Catalog_And_Search_Out_Of_PublicExperience()
    {
        Assert.Equal("PublicExperience", PublicExperienceOwnershipBoundary.SurfaceOwnerModule);
        Assert.Equal("Tour", PublicExperienceOwnershipBoundary.CatalogOwnerModule);
        Assert.Equal("Seo", PublicExperienceOwnershipBoundary.SeoOwnerModule);
        Assert.Equal("Search", PublicExperienceOwnershipBoundary.SearchOwnerModule);
        Assert.Equal("PresentationAndSeoComposition", PublicExperienceOwnershipBoundary.CompositionPosture);
    }

    [Fact]
    public void Listing_And_Landing_Are_Separate_Surfaces()
    {
        Assert.Equal("Discovery", PublicExperienceListingLandingBoundary.ListingPurpose);
        Assert.Equal("SearchIntent", PublicExperienceListingLandingBoundary.LandingPurpose);
        Assert.False(PublicExperienceListingLandingBoundary.LandingIsFilteredListing);
        Assert.Equal("/tours", PublicExperienceListingLandingBoundary.ListingRoutePattern);
        Assert.Equal("/tours/{topic}/{intent}", PublicExperienceListingLandingBoundary.LandingRoutePattern);
        Assert.Equal("/tours/{slug}", PublicExperienceListingLandingBoundary.DetailRoutePattern);
        Assert.Equal("Search", PublicExperienceListingLandingBoundary.SearchEngineOwnerModule);
        Assert.Equal("Seo", PublicExperienceListingLandingBoundary.IndexPolicyOwnerModule);
    }

    [Fact]
    public void Detail_Uses_Shared_Shell_Not_Independent_Kind_Pages()
    {
        Assert.Equal("SharedShellPlusKindSpecificSections", PublicExperienceDetailComposition.ShellPosture);
        Assert.False(PublicExperienceDetailComposition.IndependentKindPagesAllowed);
        Assert.False(PublicExperienceDetailComposition.GiantUnionViewModelAllowed);
        Assert.Contains("Itinerary", PublicExperienceDetailComposition.ExperienceSections, StringComparison.Ordinal);
        Assert.Contains("Flight", PublicExperienceDetailComposition.FuturePackageSections, StringComparison.Ordinal);
    }

    [Fact]
    public void Related_Tours_Are_Composition_Not_Recommendation()
    {
        Assert.Equal("PublicExperience", PublicExperienceRelatedToursBoundary.PresentationOwner);
        Assert.Equal("Tour", PublicExperienceRelatedToursBoundary.FactOwner);
        Assert.Equal("Search", PublicExperienceRelatedToursBoundary.FutureRetrievalOwner);
        Assert.False(PublicExperienceRelatedToursBoundary.RecommendationEngineAllowed);
        Assert.False(PublicExperienceRelatedToursBoundary.SearchRankingAllowed);
        Assert.Equal(6, PublicExperienceRelatedToursBoundary.MaxItems);
    }

    [Fact]
    public void Related_Content_Is_Composition_Not_Copied_Into_Tour()
    {
        Assert.Equal("PublicExperience", PublicExperienceRelatedContentBoundary.PresentationOwner);
        Assert.Equal("Content", PublicExperienceRelatedContentBoundary.FactOwner);
        Assert.Equal("Tour", PublicExperienceRelatedContentBoundary.CatalogFactOwner);
        Assert.Equal("Seo", PublicExperienceRelatedContentBoundary.IndexPolicyOwner);
        Assert.False(PublicExperienceRelatedContentBoundary.CopyContentIntoTourAllowed);
        Assert.False(PublicExperienceRelatedContentBoundary.ContentPublicationOwnsIndexPolicy);
        Assert.Equal(6, PublicExperienceRelatedContentBoundary.MaxItems);
    }

    [Fact]
    public void AgencyOffer_Presentation_Is_Inquiry_Only_Not_Commercial_Flow()
    {
        Assert.Equal("PublicExperience", PublicExperienceAgencyOfferBoundary.PresentationOwner);
        Assert.Equal("AgencyMarketplace", PublicExperienceAgencyOfferBoundary.FactOwner);
        Assert.Equal("Seo", PublicExperienceAgencyOfferBoundary.IndexPolicyOwner);
        Assert.Equal("Tour", PublicExperienceAgencyOfferBoundary.CatalogStatusOwner);
        Assert.False(PublicExperienceAgencyOfferBoundary.CommercialFlowAllowed);
        Assert.False(PublicExperienceAgencyOfferBoundary.AgencyPriceDisplayAllowed);
        Assert.False(PublicExperienceAgencyOfferBoundary.RankingAllowed);
        Assert.False(PublicExperienceAgencyOfferBoundary.BookingCtaAllowed);
        Assert.Equal(6, PublicExperienceAgencyOfferBoundary.MaxItems);
        Assert.Contains(
            "AgencyOfferPresentation",
            PublicExperienceDetailComposition.SharedSections,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "OfferReadinessSlot",
            PublicExperienceDetailComposition.SharedSections,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Filter_Presentation_Is_Not_Search_Faceting()
    {
        Assert.Equal("PublicExperience", PublicExperienceFilterPresentationBoundary.PresentationOwner);
        Assert.Equal("Search", PublicExperienceFilterPresentationBoundary.FutureRetrievalOwner);
        Assert.Equal("Seo", PublicExperienceFilterPresentationBoundary.IndexPolicyOwner);
        Assert.False(PublicExperienceFilterPresentationBoundary.FacetingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.RankingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FullTextSearchAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FilteredUrlIsSeoLanding);
        Assert.False(PublicExperienceFilterPresentationBoundary.FilteredUrlOwnsIndexPolicy);
        Assert.Contains(
            "Destination",
            PublicExperienceFilterPresentationBoundary.AllowedCriteria,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Ugc_Composition_Is_Presentation_Only()
    {
        Assert.Equal("PublicExperience", PublicExperienceUgcCompositionBoundary.PresentationOwner);
        Assert.Equal("Ugc", PublicExperienceUgcCompositionBoundary.FactOwner);
        Assert.Equal("Seo", PublicExperienceUgcCompositionBoundary.IndexPolicyOwner);
        Assert.Equal("Search", PublicExperienceUgcCompositionBoundary.FutureRetrievalOwner);
        Assert.False(PublicExperienceUgcCompositionBoundary.CopyUgcIntoCatalogAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(PublicExperienceUgcCompositionBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(PublicExperienceUgcCompositionBoundary.IndependentAverageRatingEngineAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.SearchEngineAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.UgcSeoPagesAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.RankingFromUgcAllowed);
        Assert.Equal(6, PublicExperienceUgcCompositionBoundary.MaxItems);
        Assert.Contains(
            "UgcComposition",
            PublicExperienceDetailComposition.SharedSections,
            StringComparison.Ordinal);
    }
}
