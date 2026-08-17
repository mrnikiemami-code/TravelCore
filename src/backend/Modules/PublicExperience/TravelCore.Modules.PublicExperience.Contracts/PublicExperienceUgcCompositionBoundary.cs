namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P16-R8: Public UGC on Tour/Place/Agency pages is composition only.
/// UGC remains fact + eligibility owner. Search is a later projection. SEO owns IndexPolicy.
/// Publicly Eligible != SEO Indexed and != automatically Search indexed.
/// </summary>
public static class PublicExperienceUgcCompositionBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "Ugc";
    public const string IndexPolicyOwner = "Seo";
    public const string FutureRetrievalOwner = "Search";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string RatingSummaryPosture = "DerivedRebuildableReadModel";
    public const bool CopyUgcIntoCatalogAllowed = false;
    public const bool PubliclyEligibleEqualsSeoIndexed = false;
    public const bool PubliclyEligibleEqualsAutomaticallySearchIndexed = false;
    public const bool IndependentAverageRatingEngineAllowed = false;
    public const bool SearchEngineAllowed = false;
    public const bool UgcSeoPagesAllowed = false;
    public const bool RankingFromUgcAllowed = false;
    public const int MaxItems = 6;
}
