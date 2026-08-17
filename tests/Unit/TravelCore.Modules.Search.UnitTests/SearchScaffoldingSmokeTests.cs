using TravelCore.Modules.Search.Contracts;
using TravelCore.Modules.Search.Domain;
using Xunit;

namespace TravelCore.Modules.Search.UnitTests;

public sealed class SearchScaffoldingSmokeTests
{
    [Fact]
    public void SearchContractsAssembly_IsLoadable()
    {
        var marker = typeof(SearchContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Search.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Search.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void SearchDomainAssembly_IsLoadable()
    {
        var marker = typeof(SearchDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Search.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Facts_Out_Of_Search()
    {
        Assert.Equal("Search", SearchOwnershipBoundary.DiscoveryOwnerModule);
        Assert.Equal("PublicExperience", SearchOwnershipBoundary.PresentationOwnerModule);
        Assert.Equal("Tour", SearchOwnershipBoundary.CatalogFactOwner);
        Assert.Equal("Content", SearchOwnershipBoundary.EditorialFactOwner);
        Assert.Equal("Pricing", SearchOwnershipBoundary.PriceFactOwner);
        Assert.Equal("AgencyMarketplace", SearchOwnershipBoundary.AgencyOfferFactOwner);
        Assert.Equal("Seo", SearchOwnershipBoundary.IndexPolicyOwner);
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
    }

    [Fact]
    public void SearchQueryContracts_Are_Shape_Only()
    {
        var request = new SearchQueryRequest(QueryText: null, LocaleCode: "fa-IR", Criteria: null);
        var response = new SearchQueryResponse([]);

        Assert.Equal("fa-IR", request.LocaleCode);
        Assert.Null(request.QueryText);
        Assert.Empty(response.Hits);
    }

    [Fact]
    public void IndexBoundary_Is_Hybrid_ReadModel_Without_Engine()
    {
        Assert.Equal("HybridReadModel", SearchIndexBoundary.ReadModelPosture);
        Assert.False(SearchIndexBoundary.SearchDocumentIsDomainEntity);
        Assert.False(SearchIndexBoundary.PhysicalEngineCommitted);
        Assert.False(SearchIndexBoundary.SqlFullTextCommitted);
        Assert.False(SearchIndexBoundary.OpenSearchCommitted);
        Assert.False(SearchIndexBoundary.ElasticsearchCommitted);
        Assert.False(SearchIndexBoundary.EmbeddingAllowed);
        Assert.False(SearchIndexBoundary.RankingEngineAllowed);
        Assert.False(SearchIndexBoundary.FacetingEngineAllowed);
        Assert.Equal("DomainModulesRemainSourceOfTruth", SearchIndexBoundary.FactOwnerPosture);
    }

    [Fact]
    public void SearchDocument_Is_ReadModel_Shape_Not_Catalog_Entity()
    {
        var sourceId = Guid.Parse("0198b3e0-0000-7000-8000-000000000011");
        var documentId = Guid.Parse("0198b3e0-0000-7000-8000-000000000012");
        var document = new SearchDocument(
            documentId,
            EntityType: "TourProduct",
            sourceId,
            SourceModule: "Tour",
            LocaleCode: "fa-IR",
            Title: "Sample",
            SearchableText: "sample text",
            StructuredAttributes: null);

        Assert.Equal("Tour", document.SourceModule);
        Assert.NotEqual(document.DocumentId, document.SourceId);
        Assert.Equal(typeof(ISearchIndex).Namespace, typeof(SearchDocument).Namespace);
    }

    [Fact]
    public void ProjectionEnvelope_Keeps_Source_Module_As_Owner()
    {
        var sourceId = Guid.Parse("0198b3e0-0000-7000-8000-000000000013");
        var envelope = new SearchProjectionEnvelope(
            new SearchProjectionSource("Content", "Article", sourceId, "fa-IR"),
            Title: "Editorial",
            SearchableText: null,
            StructuredAttributes: null);

        Assert.Equal("Content", envelope.Source.SourceModule);
        Assert.Equal(sourceId, envelope.Source.SourceId);
    }

    [Fact]
    public void Projection_Sync_Boundary_Keeps_Search_Out_Of_Domain_Transaction()
    {
        Assert.Equal("TransactionalOutboxPlusAsyncProjectionWorker", SearchProjectionSyncBoundary.SyncPosture);
        Assert.False(SearchProjectionSyncBoundary.DomainTransactionIncludesSearchWrite);
        Assert.False(SearchProjectionSyncBoundary.SearchFailureFailsDomainTransaction);
        Assert.True(SearchProjectionSyncBoundary.ProjectionMustBeRetryable);
        Assert.True(SearchProjectionSyncBoundary.ProjectionMustBeIdempotent);
        Assert.False(SearchProjectionSyncBoundary.RealQueueInfrastructureAllowed);
        Assert.False(SearchProjectionSyncBoundary.RabbitMqDependencyAllowed);
    }

    [Fact]
    public async Task ProjectionWorker_Is_Idempotent_On_Duplicate_Event()
    {
        var store = new InMemorySearchProjectionIdempotencyStore();
        var worker = new TravelCore.Modules.Search.Infrastructure.SearchProjectionWorker(store);
        var evt = new SearchProjectionEvent(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000021"),
            SourceType: "TourProduct",
            SourceId: Guid.Parse("0198b3e0-0000-7000-8000-000000000022"),
            Version: 1,
            LocaleCode: "fa-IR",
            ChangeKind: "Upsert",
            OccurredAtUtc: DateTimeOffset.Parse("2026-08-17T12:00:00Z"));

        var first = await worker.ProcessAsync(evt, TestContext.Current.CancellationToken);
        var second = await worker.ProcessAsync(evt, TestContext.Current.CancellationToken);

        Assert.True(first.Applied);
        Assert.Equal("AcceptedForProjection", first.Outcome);
        Assert.False(second.Applied);
        Assert.Equal("DuplicateSkipped", second.Outcome);
    }

    [Fact]
    public void Faceting_Boundary_Gives_Aggregation_To_Search_Meaning_To_Domain()
    {
        Assert.Equal("Search", SearchFacetingBoundary.FacetingOwnerModule);
        Assert.Equal("PublicExperience", SearchFacetingBoundary.PresentationOwnerModule);
        Assert.Equal("DomainModulesOwnAttributeMeaning", SearchFacetingBoundary.AttributeMeaningOwnerPosture);
        Assert.Equal("SearchOwnsAggregationCountingResultComposition", SearchFacetingBoundary.OwnershipPosture);
        Assert.True(SearchFacetingBoundary.OwnsAggregation);
        Assert.True(SearchFacetingBoundary.OwnsCounting);
        Assert.True(SearchFacetingBoundary.OwnsResultComposition);
        Assert.False(SearchFacetingBoundary.OwnsAttributeMeaning);
        Assert.False(SearchFacetingBoundary.OwnsSourceFacts);
        Assert.False(SearchFacetingBoundary.FacetingEngineImplemented);
        Assert.False(SearchFacetingBoundary.ElasticsearchAggregationsAllowed);
        Assert.False(SearchFacetingBoundary.RankingAllowed);
        Assert.False(SearchFacetingBoundary.RecommendationAllowed);
        Assert.False(SearchFacetingBoundary.AiModelAllowed);
        Assert.False(SearchFacetingBoundary.DomainFacetTablesAllowed);
        Assert.False(SearchFacetingBoundary.TourFacetTablesAllowed);
        Assert.False(SearchFacetingBoundary.ContentFacetTablesAllowed);
        Assert.False(SearchFacetingBoundary.PricingFacetOwnershipAllowed);
        Assert.True(SearchFacetingBoundary.StructuredFieldsRequiredForFutureFacets);
        Assert.False(SearchOwnershipBoundary.FacetingEngineAllowed);
        Assert.False(SearchIndexBoundary.FacetingEngineAllowed);
    }

    [Fact]
    public void Facet_Contracts_Are_Shape_Only_Without_Engine()
    {
        var definition = new FacetDefinition(
            Key: "difficulty",
            DisplayLabel: "Difficulty",
            AttributeKey: "Difficulty");
        var result = new FacetResult(
            Key: definition.Key,
            Values:
            [
                new FacetValue(Value: "easy", DisplayLabel: "Easy", Count: 3),
                new FacetValue(Value: "hard", DisplayLabel: "Hard", Count: 1)
            ]);

        Assert.Equal("Difficulty", definition.AttributeKey);
        Assert.Equal(2, result.Values.Count);
        Assert.Equal(3, result.Values[0].Count);
        Assert.True(
            typeof(SearchDocument).GetProperty(nameof(SearchDocument.StructuredAttributes)) is not null);
    }

    [Fact]
    public void Ranking_Boundary_Is_Deterministic_Not_Business_Policy_Or_Recommendation()
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
        Assert.False(SearchRankingBoundary.AiModelAllowed);
        Assert.False(SearchRankingBoundary.EmbeddingsAllowed);
        Assert.False(SearchRankingBoundary.VectorSearchAllowed);
        Assert.False(SearchRankingBoundary.RecommendationEngineAllowed);
        Assert.False(SearchRankingBoundary.PersonalizationAllowed);
        Assert.False(SearchRankingBoundary.UserBehaviorRankingAllowed);
        Assert.False(SearchRankingBoundary.CollaborativeFilteringAllowed);
        Assert.False(SearchRankingBoundary.ElasticsearchDependencyAllowed);
        Assert.False(SearchRankingBoundary.OpenSearchDependencyAllowed);
        Assert.True(SearchRankingBoundary.ExplainabilityMetadataAllowed);
        Assert.False(SearchOwnershipBoundary.RankingEngineAllowed);
        Assert.False(SearchIndexBoundary.RankingEngineAllowed);
        Assert.False(SearchFacetingBoundary.RankingAllowed);
    }

    [Fact]
    public void Ranking_Contracts_Are_Engine_Neutral_Shapes()
    {
        var context = new RankingContext(LocaleCode: "fa-IR", QueryText: "istanbul", Criteria: null);
        var signal = new RankingSignal(Kind: "TextualRelevance", Value: 0.8m, Source: "query");
        var candidate = new RankedCandidate(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000031"),
            [signal]);
        var result = new RankingResult(
            candidate.SourceId,
            Ordinal: 0,
            new RankingScoreMetadata(Score: 0.8m, TieBreakKey: candidate.SourceId.ToString("N"), Diagnostics: null));

        Assert.Equal("fa-IR", context.LocaleCode);
        Assert.Equal("TextualRelevance", signal.Kind);
        Assert.Equal(0, result.Ordinal);
        Assert.Equal(typeof(ISearchRanker).Namespace, typeof(RankingResult).Namespace);
    }

    [Fact]
    public void Ai_Readiness_Is_Structured_Facts_Not_Ai_Platform()
    {
        Assert.Equal("StructuredAttributableLocaleAwareFactsFirst", SearchAiReadinessBoundary.AiReadinessPosture);
        Assert.Equal("ReusableRetrievalContracts", SearchAiReadinessBoundary.ConsumerPosture);
        Assert.True(SearchAiReadinessBoundary.ExposesStructuredRetrievalFacts);
        Assert.False(SearchAiReadinessBoundary.IsAiPlatform);
        Assert.False(SearchAiReadinessBoundary.IsLlmGateway);
        Assert.False(SearchAiReadinessBoundary.IsVectorStore);
        Assert.False(SearchAiReadinessBoundary.IsRecommendationEngine);
        Assert.False(SearchAiReadinessBoundary.EmbeddingsAllowed);
        Assert.False(SearchAiReadinessBoundary.VectorDatabaseAllowed);
        Assert.False(SearchAiReadinessBoundary.VectorSearchAllowed);
        Assert.False(SearchAiReadinessBoundary.RagAllowed);
        Assert.False(SearchAiReadinessBoundary.LlmCallsAllowed);
        Assert.False(SearchAiReadinessBoundary.PromptInfrastructureAllowed);
        Assert.False(SearchAiReadinessBoundary.AiGeneratedContentAllowed);
        Assert.False(SearchAiReadinessBoundary.SemanticMlRankingAllowed);
        Assert.False(SearchAiReadinessBoundary.PersonalizationAllowed);
        Assert.False(SearchAiReadinessBoundary.MayInventDomainFacts);
        Assert.False(SearchAiReadinessBoundary.ProjectedFactsAreSearchOwnedTruth);
        Assert.True(SearchAiReadinessBoundary.LocaleIdentityRequired);
        Assert.False(SearchAiReadinessBoundary.ChatbotSpecificContractsAllowed);
        Assert.False(SearchAiReadinessBoundary.PgVectorDependencyAllowed);
        Assert.False(SearchAiReadinessBoundary.PineconeDependencyAllowed);
        Assert.False(SearchAiReadinessBoundary.QdrantDependencyAllowed);
        Assert.False(SearchIndexBoundary.EmbeddingAllowed);
        Assert.False(SearchRankingBoundary.EmbeddingsAllowed);
    }

    [Fact]
    public void Semantic_Retrieval_Snapshot_Preserves_Locale_And_Provenance()
    {
        var sourceId = Guid.Parse("0198b3e0-0000-7000-8000-000000000041");
        var documentId = Guid.Parse("0198b3e0-0000-7000-8000-000000000042");
        var provenance = new SearchFactProvenance("Tour", FactKind: "TourProduct", SourceVersion: "3");
        var snapshot = new SemanticRetrievalSnapshot(
            documentId,
            EntityType: "TourProduct",
            sourceId,
            SourceModule: "Tour",
            LocaleCode: "fa-IR",
            Title: "استانبول",
            StructuredAttributes: new Dictionary<string, string> { ["Difficulty"] = "easy" },
            SemanticReferences: ["destination:istanbul"],
            IsPubliclyEligible: true,
            Provenance: provenance);

        Assert.Equal("fa-IR", snapshot.LocaleCode);
        Assert.Equal("Tour", snapshot.Provenance!.FactOwnerModule);
        Assert.NotEqual(snapshot.DocumentId, snapshot.SourceId);
        Assert.Contains("destination:istanbul", snapshot.SemanticReferences!);
    }

    [Fact]
    public void Query_Api_Boundary_Is_Engine_Neutral_And_Not_Seo()
    {
        Assert.Equal("/api/search", SearchQueryApiBoundary.PublicRoute);
        Assert.Equal("EngineNeutralStructuredQuery", SearchQueryApiBoundary.QueryPosture);
        Assert.Equal("ContinuationReady", SearchQueryApiBoundary.PaginationPosture);
        Assert.False(SearchQueryApiBoundary.OwnsSeoLanding);
        Assert.False(SearchQueryApiBoundary.OwnsIndexPolicy);
        Assert.False(SearchQueryApiBoundary.OwnsCanonicalPolicy);
        Assert.False(SearchQueryApiBoundary.ExposesProviderQueryDsl);
        Assert.False(SearchQueryApiBoundary.ExposesIndexNames);
        Assert.False(SearchQueryApiBoundary.ExposesShardInformation);
        Assert.False(SearchQueryApiBoundary.ExposesDatabasePredicates);
        Assert.False(SearchQueryApiBoundary.ElasticsearchDependencyAllowed);
        Assert.False(SearchQueryApiBoundary.OpenSearchDependencyAllowed);
        Assert.False(SearchQueryApiBoundary.SqlFullTextAllowed);
        Assert.False(SearchQueryApiBoundary.RecommendationAllowed);
        Assert.False(SearchQueryApiBoundary.PersonalizationAllowed);
        Assert.False(SearchQueryApiBoundary.EmbeddingsAllowed);
        Assert.False(SearchQueryApiBoundary.VectorSearchAllowed);
        Assert.False(SearchQueryApiBoundary.RagAllowed);
        Assert.False(SearchQueryApiBoundary.AiSpecificEndpointAllowed);
        Assert.True(SearchQueryApiBoundary.LocaleMustBeExplicit);
        Assert.False(SearchQueryApiBoundary.AutoLanguageDetectionIsAuthoritative);
    }

    [Fact]
    public async Task Empty_Search_Query_Service_Requires_Locale_And_Returns_Stub()
    {
        var service = new TravelCore.Modules.Search.Infrastructure.Services.EmptySearchQueryService();
        var response = await service.QueryAsync(
            new SearchPublicQueryRequest(
                LocaleCode: "fa-IR",
                QueryText: "istanbul",
                EntityTypes: ["TourProduct"],
                StructuredFilters: new Dictionary<string, string> { ["Difficulty"] = "easy" },
                Sort: null,
                PageSize: 10,
                ContinuationToken: null,
                RequestedFacets: ["difficulty"]),
            TestContext.Current.CancellationToken);

        Assert.Equal("fa-IR", response.LocaleCode);
        Assert.Empty(response.Hits);
        Assert.NotNull(response.Facets);
        Assert.Equal(10, response.Continuation.PageSize);
        Assert.Equal(0, response.Continuation.ReturnedCount);
        Assert.Null(response.Continuation.NextContinuationToken);
        Assert.DoesNotContain("lucene", string.Join(' ', response.ResultMetadata!.Values), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("shard", string.Join(' ', response.ResultMetadata!.Keys), StringComparison.OrdinalIgnoreCase);

        await Assert.ThrowsAsync<ArgumentException>(() => service.QueryAsync(
            new SearchPublicQueryRequest(
                LocaleCode: " ",
                QueryText: null,
                EntityTypes: null,
                StructuredFilters: null,
                Sort: null,
                PageSize: null,
                ContinuationToken: null,
                RequestedFacets: null),
            TestContext.Current.CancellationToken));
    }

    private sealed class InMemorySearchProjectionIdempotencyStore : ISearchProjectionIdempotencyStore
    {
        private readonly HashSet<Guid> _processed = [];

        public Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(_processed.Contains(eventId));

        public Task MarkProcessedAsync(Guid eventId, long sourceVersion, CancellationToken cancellationToken = default)
        {
            _ = sourceVersion;
            _processed.Add(eventId);
            return Task.CompletedTask;
        }
    }
}
