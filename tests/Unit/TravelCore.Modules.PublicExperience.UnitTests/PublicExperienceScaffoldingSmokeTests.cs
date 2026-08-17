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
}
