namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R5: Search owns ranking composition, relevance ordering, deterministic tie-break,
/// and ranking metadata. Not business-policy authority and not a recommendation engine.
/// </summary>
public static class SearchRankingBoundary
{
    public const string RankingOwnerModule = "Search";
    public const string RankingPosture = "DeterministicExplainableSignalsPlusStableTieBreak";
    public const bool OwnsRankingComposition = true;
    public const bool OwnsRelevanceOrdering = true;
    public const bool OwnsDeterministicTieBreak = true;
    public const bool OwnsRankingResultMetadata = true;
    public const bool OwnsTourBusinessPriority = false;
    public const bool OwnsAgencyCommercialPriority = false;
    public const bool OwnsCommissionPolicy = false;
    public const bool OwnsSponsorshipPolicy = false;
    public const bool OwnsProfitabilityPolicy = false;
    public const bool OwnsCatalogTruth = false;
    public const bool RankingEngineImplemented = false;
    public const bool MachineLearningRankingAllowed = false;
    public const bool AiModelAllowed = false;
    public const bool EmbeddingsAllowed = false;
    public const bool VectorSearchAllowed = false;
    public const bool RecommendationEngineAllowed = false;
    public const bool PersonalizationAllowed = false;
    public const bool UserBehaviorRankingAllowed = false;
    public const bool CollaborativeFilteringAllowed = false;
    public const bool ElasticsearchDependencyAllowed = false;
    public const bool OpenSearchDependencyAllowed = false;
    public const bool ExplainabilityMetadataAllowed = true;
}
