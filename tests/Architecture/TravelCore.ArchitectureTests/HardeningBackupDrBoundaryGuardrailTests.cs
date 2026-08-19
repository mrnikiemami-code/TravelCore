using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T007: backup/restore / DR / DB recovery boundary without cloud backup product.
/// </summary>
public sealed class HardeningBackupDrBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void HardeningBackupDrBoundary_Is_Declared()
    {
        Assert.True(HardeningBackupDrBoundary.BackupDrBoundaryImplemented);
        Assert.Equal(
            "Backup/restore is operational posture boundary",
            HardeningBackupDrBoundary.BackupRestoreIsOperationalPosture);
        Assert.True(HardeningFoundationBoundary.BackupDrBoundaryImplemented);
        Assert.False(HardeningFoundationBoundary.BackupAutomationImplemented);
    }

    [Fact]
    public void HardeningDbRecoveryBoundary_Preserves_PostgreSql_SoR()
    {
        Assert.True(HardeningDbRecoveryBoundary.DbRecoveryBoundaryImplemented);
        Assert.Equal("PostgreSQL remains Source of Record", HardeningDbRecoveryBoundary.PostgreSqlIsSourceOfRecord);
        Assert.Equal(
            "Module-owned migrations preserved during recovery posture",
            HardeningDbRecoveryBoundary.ModuleOwnedMigrationsPreserved);
        Assert.False(HardeningDbRecoveryBoundary.PointInTimeRecoveryProductImplemented);
    }

    [Fact]
    public void Hardening_T007_Forbids_Backup_And_Recovery_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(CloudBackupClient|BackupAutomationService|RestoreDrillRunner|DisasterRecoveryOrchestrator|PointInTimeRecoveryService|DatabaseFailoverManager|MultiRegionReplicationService)\b",
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
            "Hardening T007 forbids early backup/DR product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T007_And_BackupDr_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T007", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningBackupDrBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningDbRecoveryBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R5", plan, StringComparison.Ordinal);
    }
}
