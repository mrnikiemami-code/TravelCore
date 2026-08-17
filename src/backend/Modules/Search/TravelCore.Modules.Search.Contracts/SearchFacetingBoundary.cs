namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R4: Search owns faceting aggregation / counting / result composition.
/// Domain modules own attribute meaning and source facts. PE owns filter UI only (P14-R8).
/// No facet engine implementation in T004.
/// </summary>
public static class SearchFacetingBoundary
{
    public const string FacetingOwnerModule = "Search";
    public const string PresentationOwnerModule = "PublicExperience";
    public const string AttributeMeaningOwnerPosture = "DomainModulesOwnAttributeMeaning";
    public const string OwnershipPosture = "SearchOwnsAggregationCountingResultComposition";
    public const bool OwnsAggregation = true;
    public const bool OwnsCounting = true;
    public const bool OwnsResultComposition = true;
    public const bool OwnsAttributeMeaning = false;
    public const bool OwnsSourceFacts = false;
    public const bool FacetingEngineImplemented = false;
    public const bool ElasticsearchAggregationsAllowed = false;
    public const bool RankingAllowed = false;
    public const bool RecommendationAllowed = false;
    public const bool AiModelAllowed = false;
    public const bool DomainFacetTablesAllowed = false;
    public const bool TourFacetTablesAllowed = false;
    public const bool ContentFacetTablesAllowed = false;
    public const bool PricingFacetOwnershipAllowed = false;
    public const bool StructuredFieldsRequiredForFutureFacets = true;
}
