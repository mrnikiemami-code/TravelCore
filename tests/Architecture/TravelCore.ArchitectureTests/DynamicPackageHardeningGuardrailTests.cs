using System.Reflection;
using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackageHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackageHardening_NoPeerInfraDependencies()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");

        var forbidden = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n =>
                n.Contains("Flight.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || n.Contains("HotelBooking.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Payment.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(forbidden.Count == 0, "Forbidden DynamicPackage peer infra refs:\n" + string.Join('\n', forbidden));
    }

    [Fact]
    public void DynamicPackageHardening_DoesNotOwn_Pricing()
    {
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPricing);
    }

    [Fact]
    public void DynamicPackageHardening_NoDistributedTransaction()
    {
        Assert.False(PackageOrchestrationPlan.DistributedTransactionAllowed);
        Assert.False(PackagePaymentBoundary.DistributedTransactionAllowed);
        Assert.False(PackageConfirmationBoundary.DistributedTransactionAllowed);
        Assert.False(PackagePublicJourneyBoundary.DistributedTransactionAllowed);
    }

    [Fact]
    public void DynamicPackageHardening_DoesNotContain_GenericBooking_Abstractions()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "DynamicPackage");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+"
            + @"(BookingBase|Booking<|GenericBookingAggregate)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
        hits.AddRange(
            Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(p => !IsGeneratedOrBin(p))
                .SelectMany(path => File.ReadAllLines(path)
                    .Select((line, i) => (path, line, i))
                    .Where(x => forbiddenType.IsMatch(x.line))
                    .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")));

        Assert.True(hits.Count == 0, "Forbidden generic booking abstractions:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackageHardening_DoesNotContain_GenericPayment_TargetType()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "DynamicPackage");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(TargetType|TargetId|GenericPayment|PaymentTarget<)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
        hits.AddRange(
            Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(p => !IsGeneratedOrBin(p))
                .SelectMany(path => File.ReadAllLines(path)
                    .Select((line, i) => (path, line, i))
                    .Where(x => forbiddenType.IsMatch(x.line))
                    .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")));

        Assert.True(hits.Count == 0, "Forbidden generic payment target types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackageHardening_DeferredItems_NotImplemented()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "DynamicPackage");

        var forbiddenType = new Regex(
            @"\b(PartialRefund|ComponentCancellation|MultiCity|SupplierRouting|RealSupplier|PaymentProvider|DiscountEngine)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
        hits.AddRange(
            Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(p => !IsGeneratedOrBin(p))
                .SelectMany(path => File.ReadAllLines(path)
                    .Select((line, i) => (path, line, i))
                    .Where(x => forbiddenType.IsMatch(x.line))
                    .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")));

        Assert.True(hits.Count == 0, "Deferred capability items accidentally implemented:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackageHardening_Evidence_DocsShow_ReadyForGate()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("P23 READY_FOR_GATE", text, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.EndsWith(".Designer.cs", StringComparison.Ordinal)
               || fileName.EndsWith(".g.cs", StringComparison.Ordinal)
               || fileName.EndsWith(".razor.g.cs", StringComparison.Ordinal)
               || fileName.EndsWith(".tt.g.cs", StringComparison.Ordinal)
               || fileName is "bin" or "obj";
    }
}

