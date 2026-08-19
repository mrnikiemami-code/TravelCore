using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T004: rate limiting / abuse protection boundary without middleware or WAF product.
/// </summary>
public sealed class HardeningRateLimitBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void HardeningRateLimitBoundary_Is_Declared()
    {
        Assert.True(HardeningRateLimitBoundary.RateLimitBoundaryImplemented);
        Assert.Equal(
            "Rate limiting is cross-cutting security posture",
            HardeningRateLimitBoundary.RateLimitingIsCrossCuttingPosture);
        Assert.Equal(
            "Abuse protection != authorization replacement",
            HardeningRateLimitBoundary.AbuseProtectionDoesNotReplaceAuthorization);
        Assert.True(HardeningFoundationBoundary.RateLimitBoundaryImplemented);
        Assert.False(HardeningFoundationBoundary.RateLimiterImplemented);
    }

    [Fact]
    public void Hardening_T004_Forbids_Rate_Limiter_And_Waf_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(RateLimiter|RateLimitMiddleware|SlidingWindowLimiter|TokenBucketLimiter|DistributedRateLimitStore|WafClient|DdosMitigationService|AbuseProtectionService)\b",
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
            "Hardening T004 forbids early rate-limit/WAF product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T004_And_RateLimit_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T004", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningRateLimitBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R2", plan, StringComparison.Ordinal);
    }
}
