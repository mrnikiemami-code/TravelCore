namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R6: Search is structurally AI-consumable via attributable locale-aware facts.
/// Search is not an AI platform, LLM gateway, vector store, or recommendation engine.
/// </summary>
public static class SearchAiReadinessBoundary
{
    public const string AiReadinessPosture = "StructuredAttributableLocaleAwareFactsFirst";
    public const string ConsumerPosture = "ReusableRetrievalContracts";
    public const bool ExposesStructuredRetrievalFacts = true;
    public const bool IsAiPlatform = false;
    public const bool IsLlmGateway = false;
    public const bool IsVectorStore = false;
    public const bool IsRecommendationEngine = false;
    public const bool EmbeddingsAllowed = false;
    public const bool VectorDatabaseAllowed = false;
    public const bool VectorSearchAllowed = false;
    public const bool RagAllowed = false;
    public const bool LlmCallsAllowed = false;
    public const bool PromptInfrastructureAllowed = false;
    public const bool AiGeneratedContentAllowed = false;
    public const bool SemanticMlRankingAllowed = false;
    public const bool PersonalizationAllowed = false;
    public const bool MayInventDomainFacts = false;
    public const bool ProjectedFactsAreSearchOwnedTruth = false;
    public const bool LocaleIdentityRequired = true;
    public const bool ChatbotSpecificContractsAllowed = false;
    public const bool ElasticsearchDependencyAllowed = false;
    public const bool OpenSearchDependencyAllowed = false;
    public const bool PgVectorDependencyAllowed = false;
    public const bool PineconeDependencyAllowed = false;
    public const bool QdrantDependencyAllowed = false;
}
