namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R8: Filter presentation is Public Experience only.
/// Faceting / retrieval / ranking / FTS remain P15 Search.
/// Filtered query URLs are not SEO landings and do not own IndexPolicy.
/// </summary>
public static class PublicExperienceFilterPresentationBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FutureRetrievalOwner = "Search";
    public const string IndexPolicyOwner = "Seo";
    public const string CompositionPosture = "FilterUiPlusUrlStatePlusSelection";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string AllowedCriteria = "Destination+PresentationSort";
    public const bool FacetingAllowed = false;
    public const bool RankingAllowed = false;
    public const bool FullTextSearchAllowed = false;
    public const bool FilteredUrlIsSeoLanding = false;
    public const bool FilteredUrlOwnsIndexPolicy = false;
}
