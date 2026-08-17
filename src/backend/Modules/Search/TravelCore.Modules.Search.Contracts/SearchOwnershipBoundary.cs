namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R1: Search is Retrieval + Discovery owner. Not Catalog, Content, Pricing, Agency, or SEO SoT.
/// Search may later hold a replaceable read model / projection — never write-own domain facts.
/// </summary>
public static class SearchOwnershipBoundary
{
    public const string DiscoveryOwnerModule = "Search";
    public const string PresentationOwnerModule = "PublicExperience";
    public const string CatalogFactOwner = "Tour";
    public const string EditorialFactOwner = "Content";
    public const string PriceFactOwner = "Pricing";
    public const string AgencyOfferFactOwner = "AgencyMarketplace";
    public const string IndexPolicyOwner = "Seo";
    public const string CompositionPosture = "HybridReadModelPlusQueryContracts";
    public const bool OwnsTourFacts = false;
    public const bool OwnsContentFacts = false;
    public const bool OwnsPricingFacts = false;
    public const bool OwnsAgencyFacts = false;
    public const bool OwnsIndexPolicy = false;
    public const bool RankingEngineAllowed = false;
    public const bool FacetingEngineAllowed = false;
    public const bool FullTextSearchImplemented = false;
    public const bool ElasticsearchCommitted = false;
    public const bool RecommendationEngineAllowed = false;
}
