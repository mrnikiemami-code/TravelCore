namespace TravelCore.Hardening;

/// <summary>
/// P29-R6 health/observability extension posture. Extend existing platform foundations without replacing ownership.
/// </summary>
public static class HardeningHealthObservabilityInteractionBoundary
{
    public const string HealthOwnsMinimalOperationalChecks = "Health owns minimal operational checks";
    public const string ObservabilityOwnsPlatformTelemetry = "Observability owns platform telemetry";
    public const string HardeningDoesNotReplaceHealth = "Hardening != Health replacement";
    public const string HardeningDoesNotReplaceObservability = "Hardening != Observability replacement";
    public const string ProductAnalyticsSeparate = "Product Analytics remains separate (P27 boundary)";
    public const string ErrorMonitoringPostureWithoutApmVendor = "Error monitoring posture without APM vendor lock-in";

    public const bool HealthObservabilityInteractionBoundaryImplemented = true;
    public const bool HealthProjectReferenceRequired = false;
    public const bool ObservabilityProjectReferenceRequired = false;
    public const bool ApmVendorProductImplemented = false;
    public const bool RichDiagnosticsApiImplemented = false;
}
