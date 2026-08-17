namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R3: Listing and SEO Landing are separate Public Experience surfaces.
/// Listing = Discovery. Landing = Search Intent composition.
/// Landing is not a filtered listing with a pretty URL.
/// Search engine (query / ranking / FTS) remains P15. IndexPolicy remains SEO.
/// </summary>
public static class PublicExperienceListingLandingBoundary
{
    public const string ListingPurpose = "Discovery";
    public const string LandingPurpose = "SearchIntent";
    public const bool LandingIsFilteredListing = false;

    public const string ListingRoutePattern = "/tours";
    public const string LandingRoutePattern = "/tours/{topic}/{intent}";
    public const string DetailRoutePattern = "/tours/{slug}";

    public const string ListingComposition = "FilterSlot+SortSlot+Selection";
    public const string LandingComposition = "CuratedContent+RelatedToursSlot+SeoMetadata+UserIntent";

    public const string SearchEngineOwnerModule = "Search";
    public const string IndexPolicyOwnerModule = "Seo";
}
