using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T002: production hardening foundation boundary without rate limiter/audit/secret/backup product implementation.
/// </summary>
public sealed class HardeningFoundationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void HardeningProject_Exists_With_Foundation_Boundaries()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Hardening");
        Assert.Equal("LOCKED", HardeningFoundationBoundary.SecurityFromDayOne);
        Assert.True(HardeningFoundationBoundary.SeparateHardeningFoundationImplemented);
        Assert.True(HardeningOwnershipBoundary.FoundationBoundaryImplemented);
    }

    [Fact]
    public void HardeningFoundationBoundary_Keeps_Security_Product_Deferred()
    {
        Assert.Equal("Secrets != BusinessData", HardeningFoundationBoundary.SecretsAreNotBusinessData);
        Assert.Equal("Health != RichDiagnostics", HardeningFoundationBoundary.HealthIsNotRichDiagnostics);
        Assert.Equal("AuditMetadata != AuditEventProduct", HardeningFoundationBoundary.AuditMetadataIsNotAuditEventProduct);
        Assert.False(HardeningFoundationBoundary.RateLimiterImplemented);
        Assert.False(HardeningFoundationBoundary.AuditEventStoreImplemented);
        Assert.False(HardeningFoundationBoundary.SecretManagerImplemented);
        Assert.False(HardeningFoundationBoundary.BackupAutomationImplemented);
    }

    [Fact]
    public void HardeningOwnershipBoundary_Preserves_Module_Ownership()
    {
        Assert.Equal("Hardening != Observability", HardeningOwnershipBoundary.HardeningIsNotObservability);
        Assert.Equal("Hardening != ProductAnalytics", HardeningOwnershipBoundary.HardeningIsNotProductAnalytics);
        Assert.Equal("Hardening != PerformanceOptimization", HardeningOwnershipBoundary.HardeningIsNotPerformanceOptimization);
        Assert.Equal("Hardening != MediaDelivery", HardeningOwnershipBoundary.HardeningIsNotMediaDelivery);
        Assert.Equal("Hardening != DomainAuthorization", HardeningOwnershipBoundary.HardeningIsNotDomainAuthorization);
        Assert.False(HardeningOwnershipBoundary.OwnsPlatformTelemetry);
        Assert.False(HardeningOwnershipBoundary.OwnsProductAnalytics);
        Assert.False(HardeningOwnershipBoundary.OwnsDomainAuthorizationFacts);
    }

    [Fact]
    public void P29_Evidence_Records_T002_And_Foundation_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T002", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningFoundationBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningOwnershipBoundary", plan, StringComparison.Ordinal);
    }
}
