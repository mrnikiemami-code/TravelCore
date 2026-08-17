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
}
