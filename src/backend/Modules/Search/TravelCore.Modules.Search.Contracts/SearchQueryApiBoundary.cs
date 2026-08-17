namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R7: Public Search query API is engine-neutral. Not Elasticsearch DSL, not SEO IndexPolicy, not SoT.
/// </summary>
public static class SearchQueryApiBoundary
{
    public const string PublicRoute = "/api/search";
    public const string QueryPosture = "EngineNeutralStructuredQuery";
    public const string PaginationPosture = "ContinuationReady";
    public const bool OwnsSeoLanding = false;
    public const bool OwnsIndexPolicy = false;
    public const bool OwnsCanonicalPolicy = false;
    public const bool ExposesProviderQueryDsl = false;
    public const bool ExposesIndexNames = false;
    public const bool ExposesShardInformation = false;
    public const bool ExposesDatabasePredicates = false;
    public const bool ElasticsearchDependencyAllowed = false;
    public const bool OpenSearchDependencyAllowed = false;
    public const bool SqlFullTextAllowed = false;
    public const bool RecommendationAllowed = false;
    public const bool PersonalizationAllowed = false;
    public const bool EmbeddingsAllowed = false;
    public const bool VectorSearchAllowed = false;
    public const bool RagAllowed = false;
    public const bool AiSpecificEndpointAllowed = false;
    public const bool LocaleMustBeExplicit = true;
    public const bool AutoLanguageDetectionIsAuthoritative = false;
}
