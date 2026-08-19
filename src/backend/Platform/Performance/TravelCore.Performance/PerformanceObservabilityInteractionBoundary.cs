namespace TravelCore.Performance;

/// <summary>
/// P28 measurement vs Observability separation. Platform telemetry stays in Observability; Performance consumes measurement posture only.
/// </summary>
public static class PerformanceObservabilityInteractionBoundary
{
    public const string ObservabilityOwnsPlatformTelemetry = "Observability owns platform telemetry";
    public const string PerformanceUsesMeasurementForOptimizationDecisions =
        "Performance uses measurement posture for optimization decisions";
    public const string PerformanceDoesNotReplaceObservability = "Performance != Observability replacement";
    public const string ProductAnalyticsSeparate = "Product Analytics remains separate (P27 boundary)";
    public const string NoApmVendorInPerformanceModule = "No APM vendor lock-in in Performance module";

    public const bool ObservabilityInteractionBoundaryImplemented = true;
    public const bool ObservabilityProjectReferenceRequired = false;
    public const bool ApmExporterImplemented = false;
    public const bool OpenTelemetryProductImplemented = false;
    public const bool BusinessMetricsProductImplemented = false;
}
