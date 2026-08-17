namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R2: Hybrid read-model posture. Search owns a replaceable document/index abstraction.
/// Physical engine (Elasticsearch / OpenSearch / SQL FTS) is not committed in T002.
/// </summary>
public static class SearchIndexBoundary
{
    public const string ReadModelPosture = "HybridReadModel";
    public const string DocumentKind = "SearchDocument";
    public const bool SearchDocumentIsDomainEntity = false;
    public const bool PhysicalEngineCommitted = false;
    public const bool SqlFullTextCommitted = false;
    public const bool OpenSearchCommitted = false;
    public const bool ElasticsearchCommitted = false;
    public const bool EmbeddingAllowed = false;
    public const bool RankingEngineAllowed = false;
    public const bool FacetingEngineAllowed = false;
    public const string FactOwnerPosture = "DomainModulesRemainSourceOfTruth";
}
