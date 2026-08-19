namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R6 advanced pricing evolution posture without pricing engine rewrite.
/// </summary>
public static class EvolutionAdvancedPricingBoundary
{
    public const string AdvancedPricingIsEvolutionThemeOnly = "Advanced pricing is evolution theme only";
    public const string PricingModuleRemainsSoR = "Pricing module remains pricing fact SoR";
    public const string DynamicPricingOptimizationDeferred = "Dynamic pricing optimization engine DEFERRED";
    public const string EvolutionDoesNotOwnPriceFacts = "Evolution != pricing fact owner";

    public const bool AdvancedPricingBoundaryImplemented = true;
    public const bool PricingEngineRewriteImplemented = false;
    public const bool DynamicPricingOptimizerImplemented = false;
}
