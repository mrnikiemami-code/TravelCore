namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R5 loyalty / promotions evolution posture without loyalty engine product.
/// </summary>
public static class EvolutionLoyaltyPromotionsBoundary
{
    public const string LoyaltyIsEvolutionThemeOnly = "Loyalty is evolution theme only";
    public const string PromotionsEngineDeferred = "Promotions rules engine remains DEFERRED";
    public const string LoyaltyPointsLedgerForbidden = "Loyalty points ledger forbidden in early Post-P29 tasks";
    public const string PricingPromotionsRemainModuleOwned = "Pricing/promotion facts remain module-owned";

    public const bool LoyaltyPromotionsBoundaryImplemented = true;
    public const bool LoyaltyEngineImplemented = false;
    public const bool PromotionsEngineImplemented = false;
}
