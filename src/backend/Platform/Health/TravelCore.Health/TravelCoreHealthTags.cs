namespace TravelCore.Health;

/// <summary>
/// Tag convention for health checks that participate in readiness probing.
/// </summary>
public static class TravelCoreHealthTags
{
    /// <summary>
    /// Required-dependency checks register with this tag and run only on <c>/health/ready</c>.
    /// </summary>
    public const string Ready = "ready";
}
