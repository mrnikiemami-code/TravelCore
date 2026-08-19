namespace TravelCore.Performance;

/// <summary>
/// P28-R2 data access posture. Module-owned schema access without cross-module DbContext shortcuts or unmeasured query tuning.
/// </summary>
public static class PerformanceDataAccessBoundary
{
    public const string ModuleOwnedSchemaDataAccess = "Data access remains module-schema owned";
    public const string NoCrossSchemaQueryShortcuts = "No peer-schema FK or shared DbContext shortcuts";
    public const string NoQueryOptimizationWithoutMeasurement =
        "No query optimization without measurement evidence";
    public const string NoSchemaRedesignForPerformance =
        "No schema redesign for performance without explicit task envelope";
    public const string MeasurementBeforeDataAccessTuning =
        "Data access tuning requires measurement boundary (P28-R1)";

    public const bool DataAccessBoundaryImplemented = true;
    public const bool CrossSchemaDbContextImplemented = false;
    public const bool QueryTuningProductImplemented = false;
    public const bool SchemaMigrationForPerformanceImplemented = false;
    public const bool DapperProductImplemented = false;
}
