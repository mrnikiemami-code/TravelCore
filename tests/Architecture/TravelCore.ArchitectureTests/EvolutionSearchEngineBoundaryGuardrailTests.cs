using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionSearchEngineBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void EvolutionSearchEngineBoundary_Is_Declared()
    {
        Assert.True(EvolutionSearchEngineBoundary.SearchEvolutionBoundaryImplemented);
        Assert.True(EvolutionSearchInteractionBoundary.SearchInteractionBoundaryImplemented);
        Assert.False(EvolutionSearchEngineBoundary.SearchClusterProductImplemented);
    }

    [Fact]
    public void EvolutionModule_DoesNot_Reference_Search()
    {
        var evolution = Projects.Single(p => p.Name == "TravelCore.Evolution");
        var hits = evolution.ProjectReferences
            .Where(r => r.StartsWith("TravelCore.Modules.Search", StringComparison.Ordinal))
            .ToList();
        Assert.True(hits.Count == 0, "Evolution must not reference Search:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PostP29_Evidence_Records_T004_And_Search_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T004", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionSearchEngineBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R2", plan, StringComparison.Ordinal);
    }
}
