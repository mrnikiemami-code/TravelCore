namespace TravelCore.Evolution;

/// <summary>
/// Post-P29 operational evolution posture without evolution product delivery.
/// </summary>
public static class EvolutionOperationalBoundary
{
    public const string NoFakeEvolutionClaims = "Fake evolution readiness claims are NOT ALLOWED";
    public const string NoEvolutionProductInT008 = "No evolution product delivery in T008";
    public const string OperationalEvolutionPostureOnly = "Operational evolution posture only";
    public const string AdrRequiredForMajorTransitions = "Accepted ADR required for major transitions";

    public const bool OperationalBoundaryImplemented = true;
    public const bool EvolutionDeliveryProductImplemented = false;
    public const bool PublicEvolutionApiImplemented = false;
}
