namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R4 personalization / recommendation evolution posture without ML product.
/// </summary>
public static class EvolutionPersonalizationBoundary
{
    public const string PersonalizationIsEvolutionThemeOnly = "Personalization is evolution theme only";
    public const string RecommendationEngineDeferred = "Recommendation engine remains DEFERRED";
    public const string MlModelServingForbidden = "ML model serving forbidden in early Post-P29 tasks";
    public const string DomainModulesRemainFactOwners = "Domain modules remain fact owners for personalization inputs";

    public const bool PersonalizationBoundaryImplemented = true;
    public const bool RecommendationEngineImplemented = false;
    public const bool MlModelServingImplemented = false;
}
