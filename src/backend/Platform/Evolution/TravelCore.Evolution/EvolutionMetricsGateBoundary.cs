namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R1 metrics-driven evolution gate. Production metrics gate posture without warehouse/BI product.
/// </summary>
public static class EvolutionMetricsGateBoundary
{
    public const string RealProductionMetricsRequired = "Real production metrics required before major evolution";
    public const string NoSpeculativeFeatureDelivery = "No speculative feature delivery without metrics gate";
    public const string AnalyticsProvidesProductSignals = "Analytics provides product signals; Evolution owns gate posture";
    public const string BiDashboardProductDeferred = "BI dashboard product remains DEFERRED";
    public const string FeatureFlagVendorLockInForbidden = "Feature-flag vendor lock-in forbidden in early Post-P29 tasks";

    public const bool MetricsGateBoundaryImplemented = true;
    public const bool BiDashboardProductImplemented = false;
    public const bool FeatureFlagVendorImplemented = false;
    public const bool MetricsWarehouseProductImplemented = false;
}
