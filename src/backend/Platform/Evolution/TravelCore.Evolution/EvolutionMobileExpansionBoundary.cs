namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R7 mobile / client expansion posture without native mobile app product.
/// </summary>
public static class EvolutionMobileExpansionBoundary
{
    public const string MobileAppIsEvolutionThemeOnly = "Native mobile app is evolution theme only";
    public const string WebMobileFirstPreserved = "Web mobile-first posture preserved (UI constitution)";
    public const string NativeMobileAppProductDeferred = "Native iOS/Android app product DEFERRED";
    public const string ClientExpansionRequiresEvidence = "Client expansion requires product/ops evidence";

    public const bool MobileExpansionBoundaryImplemented = true;
    public const bool NativeMobileAppImplemented = false;
    public const bool MobileBackendForFrontendProductImplemented = false;
}
