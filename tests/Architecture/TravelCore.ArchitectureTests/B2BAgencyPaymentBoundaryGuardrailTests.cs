using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T006 / P24-R6: agency payment boundary without Payment ownership transfer.
/// </summary>
public sealed class B2BAgencyPaymentBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_Payment_Boundary_Models()
    {
        Assert.NotNull(typeof(AgencyPaymentRelationshipBoundary));
        Assert.NotNull(typeof(PaymentResponsibilityReference));
        Assert.NotNull(typeof(CommercialPaymentCapabilityReference));
        Assert.NotNull(typeof(AgencyPaymentReference));
        Assert.True(B2BOwnershipBoundary.AgencyPaymentRelationshipBoundaryImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsMoneyMovement);
        Assert.False(B2BOwnershipBoundary.ModifiesPaymentTargets);
    }

    [Fact]
    public void B2B_Does_Not_Own_Payment_Execution_Or_Money_Movement()
    {
        Assert.False(AgencyPaymentRelationshipBoundary.B2BOwnsPaymentExecution);
        Assert.False(AgencyPaymentRelationshipBoundary.B2BModifiesPaymentTargets);
        Assert.False(AgencyPaymentRelationshipBoundary.B2BOwnsMoneyMovement);
        Assert.Equal("Payment", AgencyPaymentRelationshipBoundary.PaymentExecutionOwner);
    }

    [Fact]
    public void B2B_T006_Does_Not_Change_PaymentTargets_Or_Add_Financial_Product()
    {
        var paymentTargets = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "PaymentTargetKind.cs"));
        Assert.Contains("TourBooking", paymentTargets, StringComparison.Ordinal);
        Assert.Contains("HotelBooking", paymentTargets, StringComparison.Ordinal);
        Assert.Contains("FlightBooking", paymentTargets, StringComparison.Ordinal);
        Assert.DoesNotContain("Agency", paymentTargets, StringComparison.Ordinal);
        Assert.DoesNotContain("B2B", paymentTargets, StringComparison.Ordinal);

        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(Wallet|CreditLimit|CreditAccount|Settlement|Invoice|Commission|AgencyBalance)\b",
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

        Assert.True(hits.Count == 0, "B2B T006 forbids financial product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P24_Evidence_Records_T006_And_R6()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T006", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R6", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyPaymentRelationshipBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
