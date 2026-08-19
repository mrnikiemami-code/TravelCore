using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackageMonetaryBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackage_Domain_DoesNotReference_PricingInfrastructure()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        var refs = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Pricing.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Domain must not reference Pricing.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Domain_DoesNotReference_FlightInfrastructure()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        var refs = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Flight.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Domain must not reference Flight.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Domain_DoesNotReference_HotelBookingInfrastructure()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        var refs = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("HotelBooking.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Domain must not reference HotelBooking.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void DynamicPackage_Infra_DoesNotReference_PricingInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var refs = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(n => n.Contains("Pricing.Infrastructure", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(refs.Count == 0, "DynamicPackage.Infrastructure must not reference Pricing.Infrastructure:\n" + string.Join('\n', refs));
    }

    [Fact]
    public void PackageMonetarySnapshot_Is_Transient_NonPersistent()
    {
        var dbSetTypes = typeof(DynamicPackageDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        Assert.DoesNotContain(typeof(PackageMonetarySnapshot), dbSetTypes);
        Assert.DoesNotContain(typeof(TransientPackageQuote), dbSetTypes);
    }

    [Fact]
    public void OwnershipBoundary_PackageMonetaryModelImplemented_True()
    {
        Assert.True(DynamicPackageOwnershipBoundary.PackageMonetaryModelImplemented);
    }

    [Fact]
    public void OwnershipBoundary_OwnsPricing_False()
    {
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPricing);
    }

    [Fact]
    public void OwnershipBoundary_OwnsPayment_False()
    {
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPayment);
    }

    [Fact]
    public void DynamicPackageMonetaryBoundary_Evidence_DocsShow_T004Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T004 EXECUTED", text, StringComparison.Ordinal);
    }
}
