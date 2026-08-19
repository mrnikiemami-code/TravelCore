namespace TravelCore.Performance;

/// <summary>
/// P28 read optimization posture. Evidence-based read paths without ORM replacement or unjustified Dapper product.
/// </summary>
public static class PerformanceReadOptimizationBoundary
{
    public const string ReadOptimizationRequiresEvidence = "Read optimization requires measured evidence";
    public const string DapperJustifiedByEvidenceOnly =
        "Dapper only when explicitly justified by evidence";
    public const string EfRemainsWriteAndMigrationOwner = "EF Core remains write and migration owner";
    public const string NoOrmReplacement = "No ORM replacement for performance";
    public const string ReadProjectionNotWritePath = "Read projections must not enter write paths";

    public const bool ReadOptimizationBoundaryImplemented = true;
    public const bool DapperImplementationWithoutEvidence = false;
    public const bool OrmReplacementImplemented = false;
    public const bool ReadWritePathMergeImplemented = false;
    public const bool SharedReadDbContextImplemented = false;
}
