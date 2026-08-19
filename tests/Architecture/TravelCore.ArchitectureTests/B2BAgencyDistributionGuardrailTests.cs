using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T005 / P24-R5: agency distribution boundary without Booking/Pricing/Payment ownership transfer.
/// </summary>
public sealed class B2BAgencyDistributionGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_Distribution_Boundary_Models()
    {
        Assert.NotNull(typeof(AgencyDistributionBoundary));
        Assert.NotNull(typeof(SalesChannelReference));
        Assert.NotNull(typeof(DistributionCapabilityReference));
        Assert.NotNull(typeof(AgencyDistributionReference));
        Assert.True(B2BOwnershipBoundary.AgencyDistributionBoundaryImplemented);
        Assert.True(B2BOwnershipBoundary.SalesChannelReferenceImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsSalesChannelPersistence);
        Assert.False(B2BOwnershipBoundary.OwnsCommission);
    }

    [Fact]
    public void B2B_T005_Forbids_Distribution_Product_And_Ownership_Violations()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(SalesChannel|Commission|CommissionRule|Discount|AgencyPricing|Contract|Settlement|Wallet|BookingBase)\b",
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

        Assert.True(hits.Count == 0, "B2B T005 forbids distribution/sales product types:\n" + string.Join('\n', hits));
        Assert.False(AgencyDistributionBoundary.DistributionProductTablesImplemented);
        Assert.Equal("Distribution is not Sales implementation", AgencyDistributionBoundary.DistributionIsNotSalesImplementation);
    }

    [Fact]
    public void P24_Evidence_Records_T005_And_R5()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T005", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R5", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyDistributionBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
