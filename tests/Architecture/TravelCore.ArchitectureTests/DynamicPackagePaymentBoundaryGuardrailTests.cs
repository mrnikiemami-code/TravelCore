using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackagePaymentBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackage_Infra_DoesNotReference_PaymentInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var refs = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Payment.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0,
            "DynamicPackage.Infrastructure must not reference Payment.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Domain_DoesNotReference_PaymentDomain()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        var refs = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Payment.Domain", StringComparison.OrdinalIgnoreCase)
                     || n.Contains("Payment.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0,
            "DynamicPackage.Domain must not reference Payment.Domain or Payment.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void PaymentTargetKind_DoesNotContain_DynamicPackage()
    {
        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.DoesNotContain("DynamicPackage", names);
        Assert.DoesNotContain("DynamicPackageBooking", names);
    }

    [Fact]
    public void PackagePaymentBoundary_DoesNotOwnPaymentExecution()
    {
        Assert.False(PackagePaymentBoundary.OwnsPaymentExecution);
    }

    [Fact]
    public void PackagePaymentBoundary_NoNewPaymentTarget()
    {
        Assert.False(PackagePaymentBoundary.NewPaymentTargetIntroduced);
    }

    [Fact]
    public void PackagePaymentBoundary_NoDistributedTransaction()
    {
        Assert.False(PackagePaymentBoundary.DistributedTransactionAllowed);
    }

    [Fact]
    public void PackagePaymentBoundary_NoCompensation()
    {
        Assert.False(PackagePaymentBoundary.CompensationImplemented);
    }

    [Fact]
    public void OwnershipBoundary_PaymentIntegration_NotImplemented()
    {
        Assert.False(DynamicPackageOwnershipBoundary.PaymentIntegrationImplemented);
    }

    [Fact]
    public void DynamicPackagePaymentBoundary_Evidence_DocsShow_T006Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T006 EXECUTED", text, StringComparison.Ordinal);
    }
}
