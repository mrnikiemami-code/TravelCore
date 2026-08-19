namespace TravelCore.Performance;

/// <summary>
/// P28-R4 cache ownership posture. Cache and Redis remain non-authoritative helpers only.
/// </summary>
public static class PerformanceCacheBoundary
{
    public const string CacheIsNotSourceOfRecord = "Cache != Source of Record";
    public const string RedisIsNotSourceOfRecord = "Redis != Source of Record";
    public const string PlatformOwnsCacheAbstraction = "Platform owns cache abstraction contracts";
    public const string DomainModulesDoNotOwnDistributedCache =
        "Domain modules do not own distributed cache infrastructure";
    public const string NoCacheAsAuthorityPersistence = "Cache must not become authority persistence";

    public const bool CacheBoundaryImplemented = true;
    public const bool RedisClientImplemented = false;
    public const bool DistributedCacheDeploymentImplemented = false;
    public const bool CacheProviderImplemented = false;
    public const bool CacheAsAuthorityPersistenceImplemented = false;
}
