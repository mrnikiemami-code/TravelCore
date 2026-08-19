using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T006: caching boundary and cache policy architecture without Redis/cache provider product.
/// </summary>
public sealed class PerformanceCacheBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PerformanceCacheBoundary_Is_Declared()
    {
        Assert.True(PerformanceCacheBoundary.CacheBoundaryImplemented);
        Assert.Equal("Cache != Source of Record", PerformanceCacheBoundary.CacheIsNotSourceOfRecord);
        Assert.Equal("Redis != Source of Record", PerformanceCacheBoundary.RedisIsNotSourceOfRecord);
        Assert.Equal("Cache != SourceOfRecord", PerformanceFoundationBoundary.CacheIsNotSourceOfRecord);
        Assert.Equal("Redis != SourceOfRecord", PerformanceFoundationBoundary.RedisIsNotSourceOfRecord);
        Assert.False(PerformanceCacheBoundary.RedisClientImplemented);
        Assert.False(PerformanceFoundationBoundary.RedisClientImplemented);
    }

    [Fact]
    public void PerformanceCachePolicyBoundary_Declares_Eligibility_Invalidation_And_Consistency()
    {
        Assert.True(PerformanceCachePolicyBoundary.CachePolicyBoundaryImplemented);
        Assert.Equal(
            "Cache eligibility requires measurement and explicit boundary",
            PerformanceCachePolicyBoundary.CacheEligibilityRequiresMeasurement);
        Assert.Equal(
            "Invalidation principles must be declared before cache use",
            PerformanceCachePolicyBoundary.InvalidationPrinciplesRequired);
        Assert.Equal(
            "Consistency boundary must be explicit for cached reads",
            PerformanceCachePolicyBoundary.ConsistencyBoundaryExplicit);
        Assert.False(PerformanceCachePolicyBoundary.CachePolicyEngineImplemented);
        Assert.False(PerformanceCachePolicyBoundary.WriteThroughCacheAuthorityImplemented);
    }

    [Fact]
    public void Performance_T006_Forbids_Redis_And_Cache_Provider_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(IRedisConnection|RedisCacheProvider|IDistributedCacheAdapter|StackExchangeRedis|CachePolicyEngine|CacheWarmupService|DistributedInvalidationBus|CacheAuthorityStore)\b",
            RegexOptions.Compiled);

        var hits = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                var text = File.ReadAllText(path);
                return pattern.Matches(text).Select(m => $"{path}: {m.Value}");
            })
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Performance T006 forbids cache provider product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PerformanceProject_DoesNot_Reference_Redis_Or_Caching_Packages()
    {
        var csproj = File.ReadAllText(
            Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance", "TravelCore.Performance", "TravelCore.Performance.csproj"));
        Assert.DoesNotContain("StackExchange.Redis", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft.Extensions.Caching", csproj, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void P28_Evidence_Records_T006_And_Cache_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T006", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceCacheBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceCachePolicyBoundary", plan, StringComparison.Ordinal);
    }
}
