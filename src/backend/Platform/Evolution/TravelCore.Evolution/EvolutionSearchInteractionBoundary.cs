namespace TravelCore.Evolution;

/// <summary>
/// Post-P29 search evolution vs P15 Search separation.
/// </summary>
public static class EvolutionSearchInteractionBoundary
{
    public const string SearchModuleOwnsSearchFacts = "Search module owns search facts and read boundary";
    public const string EvolutionDoesNotReplaceSearchModule = "Evolution != Search module replacement";
    public const string RankingEngineRemainsDeferred = "Ranking engine remains DEFERRED unless ADR locks otherwise";

    public const bool SearchInteractionBoundaryImplemented = true;
    public const bool SearchModuleReferenceRequired = false;
    public const bool SearchRankingProductImplemented = false;
}
