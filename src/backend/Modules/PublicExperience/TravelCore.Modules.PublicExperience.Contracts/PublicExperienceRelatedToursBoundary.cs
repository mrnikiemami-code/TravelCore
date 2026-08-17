namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R5: Related Tours is presentation/composition only.
/// Retrieval stays behind a Tour public-read boundary so P15 can replace it later.
/// Related ≠ Recommendation. Related ≠ Search ranking.
/// </summary>
public static class PublicExperienceRelatedToursBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "Tour";
    public const string FutureRetrievalOwner = "Search";
    public const string RelationKind = "SharedDestination";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const bool RecommendationEngineAllowed = false;
    public const bool SearchRankingAllowed = false;
    public const int MaxItems = 6;
}
