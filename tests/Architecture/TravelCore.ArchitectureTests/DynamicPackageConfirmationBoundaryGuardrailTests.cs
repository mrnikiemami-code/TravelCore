using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackageConfirmationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackageConfirmation_Infra_DoesNotReference_PeerInfraModules()
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
    public void DynamicPackageConfirmation_Is_Transient_NonPersistent()
    {
        var dbSetEntityTypes = typeof(DynamicPackageDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        Assert.DoesNotContain(typeof(TransientPackageConfirmation), dbSetEntityTypes);
    }

    [Fact]
    public void OwnershipBoundary_ConfirmationModelImplemented_True()
    {
        Assert.True(PackageConfirmationBoundary.ConfirmationModelImplemented);
    }

    [Fact]
    public void OwnershipBoundary_DoesNotEnable_DistributedTransaction()
    {
        Assert.False(PackageConfirmationBoundary.DistributedTransactionAllowed);
    }

    [Fact]
    public void DynamicPackageConfirmation_Evidence_DocsShow_T007Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T007 EXECUTED", text, StringComparison.Ordinal);
    }

    [Fact]
    public void OwnershipBoundary_PaymentIntegration_Remains_Unimplemented()
    {
        Assert.False(DynamicPackageOwnershipBoundary.PaymentIntegrationImplemented);
    }
}

