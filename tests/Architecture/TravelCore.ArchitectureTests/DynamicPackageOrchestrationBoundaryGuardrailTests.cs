using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackageOrchestrationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackage_Infra_DoesNotReference_FlightInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var refs = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Flight.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Infrastructure must not reference Flight.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Infra_DoesNotReference_HotelBookingInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var refs = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("HotelBooking.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Infrastructure must not reference HotelBooking.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Infra_DoesNotReference_PaymentInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var refs = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Payment.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Infrastructure must not reference Payment.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void PackageOrchestrationPlan_Is_Transient_NonPersistent()
    {
        var dbSetTypes = typeof(DynamicPackageDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        Assert.DoesNotContain(typeof(PackageOrchestrationPlan), dbSetTypes);
    }

    [Fact]
    public void OwnershipBoundary_OrchestrationModelImplemented_True()
    {
        Assert.True(DynamicPackageOwnershipBoundary.OrchestrationModelImplemented);
    }

    [Fact]
    public void OwnershipBoundary_SagaModel_NotImplemented()
    {
        Assert.False(DynamicPackageOwnershipBoundary.SagaModelImplemented);
    }

    [Fact]
    public void DynamicPackageOrchestrationBoundary_Evidence_DocsShow_T005Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T005 EXECUTED", text, StringComparison.Ordinal);
    }
}
