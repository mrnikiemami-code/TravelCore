using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackagePublicJourneyGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackage_PublicJourney_OwnershipBoundary_PublicApi_RemainsFalse()
    {
        Assert.False(DynamicPackageOwnershipBoundary.PublicApiImplemented);
    }

    [Fact]
    public void DynamicPackage_PublicJourney_OwnershipBoundary_Frontend_RemainsFalse()
    {
        Assert.False(DynamicPackageOwnershipBoundary.FrontendImplemented);
    }

    [Fact]
    public void DynamicPackage_PublicJourney_DoesNotPersist_New_Entity()
    {
        var dbSetEntityTypes = typeof(DynamicPackageDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        Assert.DoesNotContain(typeof(PackagePublicJourneyBoundary), dbSetEntityTypes);
    }

    [Fact]
    public void DynamicPackage_PublicJourney_Evidence_DocsShow_T008Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T008 EXECUTED", text, StringComparison.Ordinal);
    }
}

