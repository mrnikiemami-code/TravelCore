namespace TravelCore.Performance;

/// <summary>
/// P28 infrastructure responsibility boundaries. Platform declares operational scaling posture without cloud lock-in or provisioning product.
/// </summary>
public static class PerformanceInfrastructureBoundary
{
    public const string InfrastructureResponsibilityExplicit =
        "Infrastructure responsibility boundaries must be explicit";
    public const string NoCloudProviderLockIn = "No cloud/provider lock-in in Performance module";
    public const string NoInfrastructureProvisioningProduct =
        "No infrastructure provisioning product in Performance module";
    public const string OperationalScalingDecisionsMeasured =
        "Operational scaling decisions require measurement evidence";
    public const string DeferredDistributedComplexity = "Microservice/mesh/bus/multi-region remain DEFERRED";

    public const bool InfrastructureBoundaryImplemented = true;
    public const bool CloudVendorAdapterImplemented = false;
    public const bool InfrastructureAsCodeProductImplemented = false;
    public const bool CdnImplementationInPerformanceModule = false;
    public const bool RedisImplementationInPerformanceModule = false;
}
