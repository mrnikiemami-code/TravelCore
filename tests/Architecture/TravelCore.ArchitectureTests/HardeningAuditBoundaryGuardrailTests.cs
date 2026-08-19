using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T005: audit / compliance event boundary without audit store or SIEM product.
/// </summary>
public sealed class HardeningAuditBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void HardeningAuditBoundary_Is_Declared()
    {
        Assert.True(HardeningAuditBoundary.AuditBoundaryImplemented);
        Assert.Equal("Row metadata != audit-event product", HardeningAuditBoundary.RowMetadataIsNotAuditEventProduct);
        Assert.Equal(
            "No cross-module audit mega-table without ADR",
            HardeningAuditBoundary.NoCrossModuleAuditMegaTable);
        Assert.True(HardeningFoundationBoundary.AuditBoundaryImplemented);
        Assert.False(HardeningFoundationBoundary.AuditEventStoreImplemented);
    }

    [Fact]
    public void HardeningRowMetadataInteractionBoundary_Preserves_Module_Ownership()
    {
        Assert.True(HardeningRowMetadataInteractionBoundary.RowMetadataInteractionBoundaryImplemented);
        Assert.Equal(
            "Payment audit/snapshot facts remain in Payment module",
            HardeningRowMetadataInteractionBoundary.PaymentAuditSnapshotRemainsInPayment);
        Assert.False(HardeningRowMetadataInteractionBoundary.AuditEventPersistenceImplemented);
    }

    [Fact]
    public void Hardening_T005_Forbids_Audit_Store_And_Siem_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(AuditEventStore|AuditLogRepository|SiemClient|ComplianceEventWriter|ImmutableAuditLog|CrossModuleAuditTable|AuditEventPublisher)\b",
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
            "Hardening T005 forbids early audit/SIEM product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T005_And_Audit_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T005", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningAuditBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R3", plan, StringComparison.Ordinal);
    }
}
