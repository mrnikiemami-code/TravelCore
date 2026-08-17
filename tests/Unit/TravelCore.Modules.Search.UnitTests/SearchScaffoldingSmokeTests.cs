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
}
