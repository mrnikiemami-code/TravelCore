using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class DynamicPackageSearchCompositionGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackageSearchComposition_DoesNotProjectReference_PeerInfraModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();

        Assert.True(hits.Count == 0, "Forbidden DynamicPackage peer infra refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackageSearchComposition_Is_Transient_NonPersistent()
    {
        var dbSetEntityTypes = typeof(DynamicPackageDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        Assert.DoesNotContain(typeof(TransientPackageCandidate), dbSetEntityTypes);
    }

    [Fact]
    public void DynamicPackageSearchComposition_Model_HasOnlyComponentReferences()
    {
        var props = typeof(TransientPackageCandidate).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var names = props.Select(p => p.Name).OrderBy(x => x).ToArray();

        Assert.Equal(new[] { "FlightComponent", "HotelComponent" }, names);

        Assert.Equal(typeof(FlightBookingId), props.Single(p => p.Name == "FlightComponent").PropertyType);
        Assert.Equal(typeof(HotelBookingId), props.Single(p => p.Name == "HotelComponent").PropertyType);
    }

    [Fact]
    public void DynamicPackageSearchComposition_Evidence_DocsShow_T003Executed()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);

        var text = File.ReadAllText(plan);
        Assert.Contains("TC-P23-T003 EXECUTED", text, StringComparison.Ordinal);
    }

    private static bool IsForbiddenPeer(string name) =>
        name is "TravelCore.Modules.Flight.Infrastructure"
            or "TravelCore.Modules.HotelBooking.Infrastructure";
}

