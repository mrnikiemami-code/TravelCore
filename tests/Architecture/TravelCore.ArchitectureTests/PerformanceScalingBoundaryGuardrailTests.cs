using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T007: scaling and infrastructure boundaries without K8s/CDN/Redis/sharding product.
/// </summary>
public sealed class PerformanceScalingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PerformanceScalingBoundary_Is_Declared()
    {
        Assert.True(PerformanceScalingBoundary.ScalingBoundaryImplemented);
        Assert.Equal(
            "Horizontal scaling requires measured operational need",
            PerformanceScalingBoundary.HorizontalScalingRequiresMeasuredNeed);
        Assert.Equal("No premature scaling without measurement evidence", PerformanceScalingBoundary.NoPrematureScaling);
        Assert.Equal(
            "Distributed complexity requires measured operational need",
            PerformanceFoundationBoundary.DistributedComplexityRequiresMeasuredNeed);
    }

    [Fact]
    public void PerformanceInfrastructureBoundary_Preserves_Deferred_Complexity_And_No_LockIn()
    {
        Assert.True(PerformanceInfrastructureBoundary.InfrastructureBoundaryImplemented);
        Assert.Equal("No cloud/provider lock-in in Performance module", PerformanceInfrastructureBoundary.NoCloudProviderLockIn);
        Assert.Equal(
            "Microservice/mesh/bus/multi-region remain DEFERRED",
            PerformanceInfrastructureBoundary.DeferredDistributedComplexity);
        Assert.False(PerformanceInfrastructureBoundary.CloudVendorAdapterImplemented);
        Assert.False(PerformanceInfrastructureBoundary.InfrastructureAsCodeProductImplemented);
        Assert.False(PerformanceScalingBoundary.KubernetesDeploymentImplemented);
        Assert.False(PerformanceScalingBoundary.DatabaseShardingImplemented);
    }

    [Fact]
    public void Performance_T007_Forbids_K8s_Cloud_And_Sharding_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(KubernetesDeployment|ICloudVendorAdapter|TerraformProvisioner|DatabaseShardRouter|AutoScalingOrchestrator|MultiRegionFailover|ServiceMeshControlPlane|CdnEdgeFunctionProduct)\b",
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
            "Performance T007 forbids scaling/infrastructure product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P28_Evidence_Records_T007_And_Scaling_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T007", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceScalingBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceInfrastructureBoundary", plan, StringComparison.Ordinal);
    }
}
