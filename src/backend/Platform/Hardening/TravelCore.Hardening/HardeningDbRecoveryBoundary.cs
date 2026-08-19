namespace TravelCore.Hardening;

/// <summary>
/// P29 DB recovery posture. PostgreSQL SoR recovery principles without recovery automation product.
/// </summary>
public static class HardeningDbRecoveryBoundary
{
    public const string PostgreSqlIsSourceOfRecord = "PostgreSQL remains Source of Record";
    public const string ModuleOwnedMigrationsPreserved = "Module-owned migrations preserved during recovery posture";
    public const string DbRecoveryIsBoundaryOnly = "DB recovery posture is boundary-only in early P29 tasks";
    public const string NoCrossSchemaRecoveryShortcuts = "No cross-schema recovery shortcuts without ADR";
    public const string PointInTimeRecoveryProductDeferred = "Point-in-time recovery product remains DEFERRED";

    public const bool DbRecoveryBoundaryImplemented = true;
    public const bool PointInTimeRecoveryProductImplemented = false;
    public const bool AutomatedFailoverProductImplemented = false;
}
