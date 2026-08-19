namespace TravelCore.Performance;

/// <summary>
/// P28 horizontal scaling posture. Stateless assumptions and measured scale decisions without premature infrastructure product.
/// </summary>
public static class PerformanceScalingBoundary
{
    public const string HorizontalScalingRequiresMeasuredNeed =
        "Horizontal scaling requires measured operational need";
    public const string StatelessApplicationAssumption = "Application tier assumes stateless scale-out posture";
    public const string NoPrematureScaling = "No premature scaling without measurement evidence";
    public const string SessionStateNotInApplicationTier =
        "Authoritative session/state must not live in application tier for scale-out";
    public const string ScaleOutNotMicroserviceExtraction =
        "Scale-out != automatic microservice extraction";

    public const bool ScalingBoundaryImplemented = true;
    public const bool KubernetesDeploymentImplemented = false;
    public const bool AutoScalingOrchestrationImplemented = false;
    public const bool DatabaseShardingImplemented = false;
    public const bool MultiRegionActiveActiveImplemented = false;
}
