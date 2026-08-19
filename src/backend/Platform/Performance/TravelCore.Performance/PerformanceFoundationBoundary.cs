namespace TravelCore.Performance;

/// <summary>
/// P28 foundation boundary markers. Platform-owned performance posture without premature optimization product.
/// </summary>
public static class PerformanceFoundationBoundary
{
    public const string ProfileBeforeOptimize = "LOCKED";
    public const string RedisIsNotSourceOfRecord = "Redis != SourceOfRecord";
    public const string CacheIsNotSourceOfRecord = "Cache != SourceOfRecord";
    public const string DapperJustifiedReadsOnly = "Dapper only for justified read projections";
    public const string EfOwnsWritesAndMigrations = "EF Core owns writes and migrations";
    public const string DistributedComplexityRequiresMeasuredNeed =
        "Distributed complexity requires measured operational need";

    public const bool SeparatePerformanceFoundationImplemented = true;
    public const bool MeasurementBoundaryImplemented = true;
    public const bool RuntimeBoundaryImplemented = true;
    public const bool DataAccessBoundaryImplemented = true;
    public const bool CacheBoundaryImplemented = true;
    public const bool ScalingBoundaryImplemented = true;
    public const bool RedisClientImplemented = false;
    public const bool CachePolicyImplemented = false;
    public const bool CdnIntegrationImplemented = false;
    public const bool LoadTestHarnessImplemented = false;
}
