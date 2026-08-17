using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
using TravelCore.Modules.Search.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P15-T001..T006: Search Discovery owner with hybrid read-model, outbox projection,
/// faceting, deterministic ranking, and AI-readiness contracts. No FTS/ML/vector/LLM engines.
/// </summary>
public sealed class SearchBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void SearchProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Infrastructure");
        Assert.Equal(
            "search",
            TravelCore.Modules.Search.Infrastructure.SearchDbContext.SchemaName);
    }

    [Fact]
    public void Search_DoesNot_Own_Peer_Facts_Or_Engines()
    {
        Assert.Equal("Search", SearchOwnershipBoundary.DiscoveryOwnerModule);
        Assert.False(SearchOwnershipBoundary.OwnsTourFacts);
        Assert.False(SearchOwnershipBoundary.OwnsContentFacts);
        Assert.False(SearchOwnershipBoundary.OwnsPricingFacts);
        Assert.False(SearchOwnershipBoundary.OwnsAgencyFacts);
        Assert.False(SearchOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(SearchOwnershipBoundary.RankingEngineAllowed);
        Assert.False(SearchOwnershipBoundary.FacetingEngineAllowed);
        Assert.False(SearchOwnershipBoundary.FullTextSearchImplemented);
        Assert.False(SearchOwnershipBoundary.ElasticsearchCommitted);
        Assert.False(SearchOwnershipBoundary.RecommendationEngineAllowed);
        Assert.Equal("HybridReadModel", SearchIndexBoundary.ReadModelPosture);
        Assert.False(SearchIndexBoundary.SearchDocumentIsDomainEntity);
        Assert.False(SearchIndexBoundary.PhysicalEngineCommitted);
        Assert.False(SearchIndexBoundary.SqlFullTextCommitted);
        Assert.False(SearchIndexBoundary.OpenSearchCommitted);
        Assert.False(SearchIndexBoundary.ElasticsearchCommitted);
        Assert.False(SearchIndexBoundary.EmbeddingAllowed);
        Assert.False(SearchIndexBoundary.RankingEngineAllowed);
        Assert.False(SearchIndexBoundary.FacetingEngineAllowed);
    }

    [Fact]
    public void SearchInfrastructure_MustNotImplement_ISearchIndex()
    {
        var infraRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Search",
            "TravelCore.Modules.Search.Infrastructure");
        Assert.True(Directory.Exists(infraRoot), infraRoot);

        var hits = Directory.EnumerateFiles(infraRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(x.line, @"\bclass\s+\w+[^{:]*:\s*[^{]*\bISearchIndex\b")
                           || Regex.IsMatch(x.line, @"\bDbSet\s*<\s*SearchDocument\s*>");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T002 forbids a concrete index engine and SearchDocument persistence:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Search.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Search.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Contracts");
        Assert.Empty(contracts.ProjectReferences);
    }

    [Fact]
    public void SearchModule_Forbids_Engine_And_Fact_Identifiers()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");
        Assert.True(Directory.Exists(root), root);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(pg_trgm|to_tsvector|ts_rank|Elasticsearch|OpenSearch|Booking|Payment|PriceOverride|Commission|SetIndexPolicy|IEmbedding|OpenAI|RabbitMQ|IConnectionFactory|pgvector|Pinecone|Qdrant|ChatCompletion|LangChain)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Search scaffolding must not implement FTS/engines/brokers or steal facts:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Search_Projection_Sync_Is_Outbox_Plus_Async_Worker()
    {
        Assert.Equal("TransactionalOutboxPlusAsyncProjectionWorker", SearchProjectionSyncBoundary.SyncPosture);
        Assert.False(SearchProjectionSyncBoundary.DomainTransactionIncludesSearchWrite);
        Assert.False(SearchProjectionSyncBoundary.SearchFailureFailsDomainTransaction);
        Assert.True(SearchProjectionSyncBoundary.ProjectionMustBeRetryable);
        Assert.True(SearchProjectionSyncBoundary.ProjectionMustBeIdempotent);
        Assert.False(SearchProjectionSyncBoundary.RealQueueInfrastructureAllowed);
        Assert.False(SearchProjectionSyncBoundary.RabbitMqDependencyAllowed);
        Assert.True(typeof(TravelCore.Modules.Search.Infrastructure.SearchProjectionWorker)
            .GetInterfaces()
            .Contains(typeof(ISearchProjectionWorker)));
    }

    [Fact]
    public void Search_Owns_Faceting_Aggregation_Not_Attribute_Meaning_Or_Engine()
    {
        Assert.Equal("Search", SearchFacetingBoundary.FacetingOwnerModule);
        Assert.Equal("PublicExperience", SearchFacetingBoundary.PresentationOwnerModule);
        Assert.True(SearchFacetingBoundary.OwnsAggregation);
        Assert.True(SearchFacetingBoundary.OwnsCounting);
        Assert.True(SearchFacetingBoundary.OwnsResultComposition);
        Assert.False(SearchFacetingBoundary.OwnsAttributeMeaning);
        Assert.False(SearchFacetingBoundary.OwnsSourceFacts);
        Assert.False(SearchFacetingBoundary.FacetingEngineImplemented);
        Assert.False(SearchFacetingBoundary.ElasticsearchAggregationsAllowed);
        Assert.False(SearchFacetingBoundary.TourFacetTablesAllowed);
        Assert.False(SearchFacetingBoundary.ContentFacetTablesAllowed);
        Assert.False(SearchFacetingBoundary.PricingFacetOwnershipAllowed);
        Assert.True(SearchFacetingBoundary.StructuredFieldsRequiredForFutureFacets);
        Assert.False(SearchOwnershipBoundary.FacetingEngineAllowed);
        Assert.False(SearchIndexBoundary.FacetingEngineAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FacetingAllowed);
    }

    [Fact]
    public void SearchInfrastructure_MustNotImplement_FacetEngine_Or_DomainFacetTables()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");
        Assert.True(Directory.Exists(root), root);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                               x.line,
                               @"\b(TermsAggregation|FacetEngine|IFacetEngine|Elasticsearch\.Net|Nest\.|OpenSearch\.Client)\b")
                           || Regex.IsMatch(x.line, @"\bDbSet\s*<\s*Facet(Definition|Value|Result)\s*>")
                           || Regex.IsMatch(x.line, @"\b(TourFacet|ContentFacet|PricingFacet)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T004 forbids facet engines, ES aggregations, and domain facet tables:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Search_Owns_Deterministic_Ranking_Not_Business_Policy_Or_Recommendation()
    {
        Assert.Equal("Search", SearchRankingBoundary.RankingOwnerModule);
        Assert.Equal("DeterministicExplainableSignalsPlusStableTieBreak", SearchRankingBoundary.RankingPosture);
        Assert.True(SearchRankingBoundary.OwnsRankingComposition);
        Assert.True(SearchRankingBoundary.OwnsRelevanceOrdering);
        Assert.True(SearchRankingBoundary.OwnsDeterministicTieBreak);
        Assert.True(SearchRankingBoundary.OwnsRankingResultMetadata);
        Assert.False(SearchRankingBoundary.OwnsTourBusinessPriority);
        Assert.False(SearchRankingBoundary.OwnsAgencyCommercialPriority);
        Assert.False(SearchRankingBoundary.OwnsCommissionPolicy);
        Assert.False(SearchRankingBoundary.OwnsSponsorshipPolicy);
        Assert.False(SearchRankingBoundary.OwnsProfitabilityPolicy);
        Assert.False(SearchRankingBoundary.OwnsCatalogTruth);
        Assert.False(SearchRankingBoundary.RankingEngineImplemented);
        Assert.False(SearchRankingBoundary.MachineLearningRankingAllowed);
        Assert.False(SearchRankingBoundary.EmbeddingsAllowed);
        Assert.False(SearchRankingBoundary.VectorSearchAllowed);
        Assert.False(SearchRankingBoundary.RecommendationEngineAllowed);
        Assert.False(SearchRankingBoundary.PersonalizationAllowed);
        Assert.False(SearchOwnershipBoundary.RankingEngineAllowed);
        Assert.False(SearchIndexBoundary.RankingEngineAllowed);
        Assert.False(SearchFacetingBoundary.RankingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.RankingAllowed);
    }

    [Fact]
    public void SearchInfrastructure_MustNotImplement_ISearchRanker_Or_MlRanking()
    {
        var infraRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Search",
            "TravelCore.Modules.Search.Infrastructure");
        Assert.True(Directory.Exists(infraRoot), infraRoot);

        var hits = Directory.EnumerateFiles(infraRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(x.line, @"\bclass\s+\w+[^{:]*:\s*[^{]*\bISearchRanker\b")
                           || Regex.IsMatch(
                               x.line,
                               @"\b(BestAgency|PreferredSeller|MostProfitable|CommissionBoost|SponsoredWinner|CollaborativeFilter|EmbeddingModel|VectorIndex)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T005 forbids concrete ranking engines and business-policy invents:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Search_Ai_Readiness_Is_Structured_Facts_Without_Vector_Or_Llm()
    {
        Assert.Equal("StructuredAttributableLocaleAwareFactsFirst", SearchAiReadinessBoundary.AiReadinessPosture);
        Assert.True(SearchAiReadinessBoundary.ExposesStructuredRetrievalFacts);
        Assert.False(SearchAiReadinessBoundary.IsAiPlatform);
        Assert.False(SearchAiReadinessBoundary.IsLlmGateway);
        Assert.False(SearchAiReadinessBoundary.IsVectorStore);
        Assert.False(SearchAiReadinessBoundary.EmbeddingsAllowed);
        Assert.False(SearchAiReadinessBoundary.VectorDatabaseAllowed);
        Assert.False(SearchAiReadinessBoundary.VectorSearchAllowed);
        Assert.False(SearchAiReadinessBoundary.RagAllowed);
        Assert.False(SearchAiReadinessBoundary.LlmCallsAllowed);
        Assert.False(SearchAiReadinessBoundary.AiGeneratedContentAllowed);
        Assert.False(SearchAiReadinessBoundary.MayInventDomainFacts);
        Assert.False(SearchAiReadinessBoundary.ProjectedFactsAreSearchOwnedTruth);
        Assert.True(SearchAiReadinessBoundary.LocaleIdentityRequired);
        Assert.False(SearchAiReadinessBoundary.ChatbotSpecificContractsAllowed);
        Assert.False(SearchAiReadinessBoundary.PgVectorDependencyAllowed);
        Assert.False(SearchAiReadinessBoundary.PineconeDependencyAllowed);
        Assert.False(SearchAiReadinessBoundary.QdrantDependencyAllowed);
        Assert.False(SearchIndexBoundary.EmbeddingAllowed);
        Assert.False(SearchRankingBoundary.EmbeddingsAllowed);
        Assert.False(SearchRankingBoundary.VectorSearchAllowed);
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Tour", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Content.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Content", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Pricing", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".AgencyMarketplace.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".AgencyMarketplace", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Seo.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Seo", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
