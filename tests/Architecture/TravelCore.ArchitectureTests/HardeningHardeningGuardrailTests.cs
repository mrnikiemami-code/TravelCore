using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T008/T009/GATE: operational hardening, deferred scope, and evidence pack locks.
/// </summary>
public sealed class HardeningHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Hardening_Guardrails_Are_Declared()
    {
        Assert.True(HardeningOwnershipBoundary.HardeningGuardrailsImplemented);
        Assert.True(HardeningOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(HardeningDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
    }

    [Fact]
    public void Hardening_T008_Locks_Accepted_Boundaries_T002_Through_T007()
    {
        Assert.True(HardeningFoundationBoundary.SeparateHardeningFoundationImplemented);
        Assert.True(HardeningFoundationBoundary.SecurityBoundaryImplemented);
        Assert.True(HardeningFoundationBoundary.RateLimitBoundaryImplemented);
        Assert.True(HardeningFoundationBoundary.AuditBoundaryImplemented);
        Assert.True(HardeningFoundationBoundary.FileSecurityBoundaryImplemented);
        Assert.True(HardeningFoundationBoundary.BackupDrBoundaryImplemented);
        Assert.True(HardeningSecurityBoundary.SecurityBoundaryImplemented);
        Assert.True(HardeningRateLimitBoundary.RateLimitBoundaryImplemented);
        Assert.True(HardeningAuditBoundary.AuditBoundaryImplemented);
        Assert.True(HardeningContentSanitizationBoundary.ContentSanitizationBoundaryImplemented);
        Assert.True(HardeningBackupDrBoundary.BackupDrBoundaryImplemented);
    }

    [Fact]
    public void HardeningOperationalBoundary_Forbids_Fake_Security_Claims_And_Product()
    {
        Assert.Equal("Fake security/compliance claims are NOT ALLOWED", HardeningOperationalBoundary.NoFakeSecurityClaims);
        Assert.Equal("No production hardening product in T008", HardeningOperationalBoundary.NoProductionHardeningProductInT008);
        Assert.False(HardeningOperationalBoundary.ApmVendorProductImplemented);
        Assert.False(HardeningOperationalBoundary.SecretManagerIntegrationImplemented);
        Assert.False(HardeningOperationalBoundary.CiPipelineYamlImplemented);
    }

    [Fact]
    public void HardeningDeferredScopeBoundary_Keeps_Vendors_And_Tooling_Deferred()
    {
        Assert.Equal("DEFERRED", HardeningDeferredScopeBoundary.PenetrationTestingVendorEngagement);
        Assert.Equal("DEFERRED", HardeningDeferredScopeBoundary.SiemCentralizedLogAggregation);
        Assert.Equal("DEFERRED", HardeningDeferredScopeBoundary.SecretManagerVendorIntegration);
        Assert.Equal("DEFERRED", HardeningDeferredScopeBoundary.CiCdYamlProduct);
        Assert.Equal("DEFERRED", HardeningDeferredScopeBoundary.MalwareAvScannerProduct);
        Assert.False(HardeningDeferredScopeBoundary.PenetrationTestVendorImplemented);
        Assert.False(HardeningDeferredScopeBoundary.SecretManagerVendorImplemented);
    }

    [Fact]
    public void HardeningHealthObservabilityInteractionBoundary_Preserves_Platform_Ownership()
    {
        Assert.True(HardeningHealthObservabilityInteractionBoundary.HealthObservabilityInteractionBoundaryImplemented);
        Assert.Equal("Hardening != Observability", HardeningOwnershipBoundary.HardeningIsNotObservability);
        Assert.Equal("Hardening != ProductAnalytics", HardeningOwnershipBoundary.HardeningIsNotProductAnalytics);
        Assert.False(HardeningHealthObservabilityInteractionBoundary.RichDiagnosticsApiImplemented);
    }

    [Fact]
    public void HardeningDeploymentSecretsBoundary_Keeps_Secrets_Out_Of_Business_Data()
    {
        Assert.True(HardeningDeploymentSecretsBoundary.DeploymentSecretsBoundaryImplemented);
        Assert.Equal("Secrets never persist in business tables", HardeningDeploymentSecretsBoundary.NoSecretsInBusinessTables);
        Assert.False(HardeningDeploymentSecretsBoundary.SecretManagerVendorImplemented);
        Assert.False(HardeningFoundationBoundary.SecretManagerImplemented);
    }

    [Fact]
    public void HardeningProductionVerificationBoundary_Is_Posture_Only()
    {
        Assert.True(HardeningProductionVerificationBoundary.ProductionVerificationBoundaryImplemented);
        Assert.Equal("Build PASS != production-ready claim", HardeningProductionVerificationBoundary.BuildPassIsNotProductionReadyClaim);
        Assert.False(HardeningProductionVerificationBoundary.ProductionSeoAuditProductImplemented);
        Assert.False(HardeningProductionVerificationBoundary.AccessibilityAuditProductImplemented);
    }

    [Fact]
    public void Hardening_T008_Forbids_Deferred_And_Ops_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(ApmVendorAdapter|SecretManagerClient|CiPipelineDefinition|PenetrationTestRunner|SiemClient|ProductionSeoAuditor|AccessibilityAuditService|RunbookAutomationEngine|HardeningAdminController|PublicHardeningController)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("///", StringComparison.Ordinal)
                        && forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Hardening T008 forbids deferred/ops product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T008_And_R6_R7_R8()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T008", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R6", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R7", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R8", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningOperationalBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningDeferredScopeBoundary", plan, StringComparison.Ordinal);
    }

    [Fact]
    public void P29_Evidence_Pack_Locks_T009_Hardening_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P29-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TC-P29-T009",
            "P29-R1",
            "P29-R8",
            "Security from day one",
            "Secrets != business data",
            "Hardening != Observability",
            "Hardening != ProductAnalytics",
            "Malware/AV scanning DEFERRED",
            "READY_FOR_GATE",
            "TC-P29-GATE",
            "NOT EXECUTED",
        ];

        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void P29_Gate_Evidence_Locks_Acceptance_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P29-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TC-P29-GATE",
            "P29 COMPLETE",
            "P29-R1",
            "P29-R8",
            "TC-P29-T009",
            "No new Hardening product capability",
            "Post-P29",
            "NOT STARTED",
        ];

        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }
}
