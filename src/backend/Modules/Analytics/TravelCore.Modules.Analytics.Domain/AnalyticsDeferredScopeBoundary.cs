namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R8 deferred/out-of-scope posture. Warehouse, BI, ML, streaming, and identity graph remain deferred unless explicitly locked.
/// </summary>
public static class AnalyticsDeferredScopeBoundary
{
    public const string DataWarehouse = "DEFERRED";
    public const string BiDashboards = "DEFERRED";
    public const string MlRecommendation = "DEFERRED";
    public const string RealTimeStreamingAnalytics = "DEFERRED";
    public const string CrossVendorIdentityGraph = "DEFERRED";

    public const bool DeferredScopeBoundaryImplemented = true;
    public const bool WarehouseConnectorImplemented = false;
    public const bool BiDashboardImplemented = false;
    public const bool MlRecommendationImplemented = false;
    public const bool StreamingPipelineImplemented = false;
    public const bool IdentityGraphImplemented = false;
}
