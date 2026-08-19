using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-Post-P29-T002: continuous evolution foundation boundary without microservice/search/mobile product.
/// </summary>
public sealed class EvolutionFoundationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void EvolutionProject_Exists_With_Foundation_Boundaries()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Evolution");
        Assert.Equal("Metrics before major evolution is mandatory", EvolutionFoundationBoundary.MetricsBeforeMajorEvolution);
        Assert.True(EvolutionFoundationBoundary.SeparateEvolutionFoundationImplemented);
        Assert.True(EvolutionOwnershipBoundary.FoundationBoundaryImplemented);
    }

    [Fact]
    public void EvolutionFoundationBoundary_Keeps_Microservice_And_Search_Product_Deferred()
    {
        Assert.Equal(
            "Microservice extraction requires evidence and Accepted ADR",
            EvolutionFoundationBoundary.MicroserviceExtractionRequiresEvidenceAndAdr);
        Assert.Equal("Modular Monolith preserved by default", EvolutionFoundationBoundary.ModularMonolithPreservedByDefault);
        Assert.False(EvolutionFoundationBoundary.MicroserviceExtractionImplemented);
        Assert.False(EvolutionFoundationBoundary.SearchClusterImplemented);
        Assert.False(EvolutionFoundationBoundary.MobileAppProductImplemented);
    }

    [Fact]
    public void EvolutionOwnershipBoundary_Preserves_Module_Ownership()
    {
        Assert.Equal("Evolution != SearchRanking", EvolutionOwnershipBoundary.EvolutionIsNotSearchRanking);
        Assert.Equal("Evolution != ProductAnalytics", EvolutionOwnershipBoundary.EvolutionIsNotProductAnalytics);
        Assert.Equal("Evolution != PricingSoR", EvolutionOwnershipBoundary.EvolutionIsNotPricingSoR);
        Assert.False(EvolutionOwnershipBoundary.OwnsSearchRanking);
        Assert.False(EvolutionOwnershipBoundary.OwnsDomainModules);
    }

    [Fact]
    public void PostP29_Evidence_Records_T002_And_Foundation_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T002", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionFoundationBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionOwnershipBoundary", plan, StringComparison.Ordinal);
    }
}
