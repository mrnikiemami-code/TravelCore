using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T007 / P24-R7: operational boundary without API/mutation/authz ownership.
/// </summary>
public sealed class B2BAgencyOperationalBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_Operational_Boundary_Models()
    {
        Assert.NotNull(typeof(AgencyOperationalBoundary));
        Assert.NotNull(typeof(AgencyReportingReference));
        Assert.NotNull(typeof(AgencyOperationalCapabilityReference));
        Assert.NotNull(typeof(AgencyOperationalReference));
        Assert.True(B2BOwnershipBoundary.AgencyOperationalBoundaryImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsOperationalAuthorization);
        Assert.False(B2BOwnershipBoundary.ExposesOperationalMutation);
    }

    [Fact]
    public void B2B_Does_Not_Own_Authorization_Or_Execution_Mutation()
    {
        Assert.False(AgencyOperationalBoundary.B2BOwnsAuthorization);
        Assert.False(AgencyOperationalBoundary.B2BExposesOperationalMutation);
        Assert.False(AgencyOperationalBoundary.B2BModifiesBookingOperations);
        Assert.False(AgencyOperationalBoundary.B2BModifiesPaymentOperations);
    }

    [Fact]
    public void B2B_T007_Forbids_Operational_Product_Surfaces()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(AdminApi|Dashboard|ReportingEngine|AuditSystem|UserManagement|PermissionPolicy)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "B2B T007 forbids operational product types:\n" + string.Join('\n', hits));
        Assert.False(AgencyOperationalBoundary.AdminApiImplemented);
        Assert.False(AgencyOperationalBoundary.PublicApiImplemented);
        Assert.False(AgencyOperationalBoundary.DashboardImplemented);
    }

    [Fact]
    public void P24_Evidence_Records_T007_And_R7()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T007", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R7", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyOperationalBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
