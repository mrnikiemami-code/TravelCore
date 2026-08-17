namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// P16-R8: UGC is the user-generated fact owner, including public-eligibility truth.
/// PublicExperience composes eligible facts only. Search may later project. SEO owns IndexPolicy.
/// </summary>
public static class UgcPublicCompositionBoundary
{
    public const string FactOwner = "Ugc";
    public const string PresentationOwner = "PublicExperience";
    public const string SearchOwner = "Search";
    public const string IndexPolicyOwner = "Seo";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string RatingSummaryPosture = "DerivedRebuildableReadModel";
    public const bool PubliclyEligibleEqualsSeoIndexed = false;
    public const bool PubliclyEligibleEqualsAutomaticallySearchIndexed = false;
    public const bool IndependentAverageRatingEngineAllowed = false;
    public const bool SearchEngineInThisTaskAllowed = false;
    public const bool UgcOwnedSeoPagesAllowed = false;
    public const bool CopyUgcIntoCatalogAllowed = false;
}
