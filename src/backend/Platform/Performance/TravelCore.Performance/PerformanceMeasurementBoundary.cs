namespace TravelCore.Performance;

/// <summary>
/// P28-R1 measurement foundation. Profile-before-optimize posture without optimization product or benchmark claims.
/// </summary>
public static class PerformanceMeasurementBoundary
{
    public const string ProfileBeforeOptimizeMandatory = "Profile before optimize is mandatory";
    public const string NoSpeculativeTuning = "No speculative tuning without measured evidence";
    public const string NoBenchmarkClaimsWithoutEvidence = "No benchmark claims without evidence";
    public const string MeasurementFoundationNotOptimizationProduct =
        "Measurement foundation != optimization implementation";
    public const string NoProductionTuningWithoutMeasurement =
        "No production tuning without measurement posture";

    public const bool MeasurementBoundaryImplemented = true;
    public const bool ApmVendorLockInImplemented = false;
    public const bool ProductionTuningAutomationImplemented = false;
    public const bool BenchmarkHarnessProductImplemented = false;
    public const bool LoadTestHarnessProductImplemented = false;
}
