using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T004 / P24-R4: agency commercial profile boundary without financial/booking execution ownership.
/// </summary>
public sealed class B2BAgencyCommercialProfileGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_Commercial_Profile_Boundary_Models()
    {
        Assert.NotNull(typeof(AgencyCommercialProfileBoundary));
        Assert.NotNull(typeof(AgencyBusinessReference));
        Assert.NotNull(typeof(CommercialCapabilityReference));
        Assert.True(B2BOwnershipBoundary.AgencyCommercialProfileBoundaryImplemented);
        Assert.True(B2BOwnershipBoundary.AgencyBusinessReferenceImplemented);
        Assert.True(B2BOwnershipBoundary.CommercialCapabilityReferenceImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsFinancialExecution);
        Assert.False(B2BOwnershipBoundary.OwnsSettlementExecution);
        Assert.False(B2BOwnershipBoundary.OwnsPricingAuthority);
    }

    [Fact]
    public void B2B_Does_Not_Own_Financial_Booking_Or_Pricing_Execution()
    {
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsFinancialExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsPaymentExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsBookingExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsPricingAuthority);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsSettlementExecution);
        Assert.Equal("Pricing", AgencyCommercialProfileBoundary.PricingAuthorityOwner);
        Assert.Equal("Booking", AgencyCommercialProfileBoundary.BookingExecutionOwner);
        Assert.Equal("Payment", AgencyCommercialProfileBoundary.PaymentExecutionOwner);
    }

    [Fact]
    public void B2B_T004_Forbids_Commercial_Product_And_Financial_Execution()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(Agency|Contract|Commission|CommissionRule|CreditLimit|Wallet|Settlement|Invoice|BookingBase|GenericBookingAggregate|IPaymentService)\b",
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

        Assert.True(hits.Count == 0, "B2B T004 forbids commercial/financial product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2B_T004_Has_No_Commercial_Business_Tables()
    {
        Assert.False(AgencyCommercialProfileBoundary.CommercialTablesImplemented);
        Assert.False(AgencyCommercialProfileBoundary.ContractImplemented);
        Assert.False(AgencyCommercialProfileBoundary.SettlementImplemented);
    }

    [Fact]
    public void P24_Evidence_Records_T004_And_R4()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T004", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R4", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyCommercialProfileBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyBusinessReference", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
