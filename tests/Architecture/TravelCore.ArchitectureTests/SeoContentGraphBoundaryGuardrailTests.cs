using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P26-T004 / P26-R1: SEO-owned content graph foundation in schema seo;
/// Content/Destination/Search ownership unchanged.
/// </summary>
public sealed class SeoContentGraphBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void SeoContentGraphBoundary_Is_Declared_In_Contracts()
    {
        Assert.Equal("Seo", SeoContentGraphOwnershipBoundary.OwnerModule);
        Assert.Equal("seo", SeoContentGraphOwnershipBoundary.SchemaName);
        Assert.Equal("seo", SeoDbContext.SchemaName);
        Assert.True(SeoContentGraphOwnershipBoundary.ContentGraphFoundationImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.PublicGraphMutationApiImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.ContentEditorialSoRImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.DestinationHierarchySoRImplemented);
        Assert.False(SeoContentGraphOwnershipBoundary.SearchRankingSoRImplemented);
    }

    [Fact]
    public void SeoResourcePublisherBoundary_Preserves_Ownership_Posture()
    {
        Assert.Equal("Content", SeoResourcePublisherBoundary.ContentPublisherOwner);
        Assert.Equal("Destination", SeoResourcePublisherBoundary.DestinationPublisherOwner);
        Assert.Equal("Search", SeoResourcePublisherBoundary.SearchIndexOwner);
        Assert.Equal("Seo", SeoResourcePublisherBoundary.GraphMechanicsOwner);
    }

    [Fact]
    public void Content_Destination_And_Search_DoNot_Depend_On_SeoGraphFoundation_For_SoR()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Content.Contracts",
                     "TravelCore.Modules.Content.Domain",
                     "TravelCore.Modules.Content.Infrastructure",
                     "TravelCore.Modules.Destination.Contracts",
                     "TravelCore.Modules.Destination.Domain",
                     "TravelCore.Modules.Destination.Infrastructure",
                     "TravelCore.Modules.Search.Contracts",
                     "TravelCore.Modules.Search.Domain",
                     "TravelCore.Modules.Search.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.Contains("SeoContentGraph", StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not depend on SEO graph foundation types:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void SeoInfrastructure_MustNotProjectReference_ContentDestinationSearchInfrastructure()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Seo.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name is "TravelCore.Modules.Content.Infrastructure"
                    or "TravelCore.Modules.Content.Domain"
                    or "TravelCore.Modules.Destination.Infrastructure"
                    or "TravelCore.Modules.Destination.Domain"
                    or "TravelCore.Modules.Search.Infrastructure"
                    or "TravelCore.Modules.Search.Domain")
            .ToList();
        Assert.True(hits.Count == 0, "Seo.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SeoModule_T004_Forbids_Early_Graph_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Seo");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(SeoHubNode|SeoClusterNode|SeoInternalLinkEdge|SeoProgrammaticLandingFactory|SeoLinkGraphCrawler|SeoGraphMutationApi|IContentGraphEditor|SeoEditorialBody)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("///", StringComparison.Ordinal)
                        && forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "Seo T004 forbids early graph product entities:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SeoModule_Forbids_PeerSchemaFk_And_SharedDbContext()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Seo");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(content|destination|search|tour|place|media)""|HasOne<.*(Content|Destination|Search|Tour|Place|Media)|TravelCore\.Modules\.(Content|Destination|Search|Tour|Place|Media)\.(Domain|Infrastructure)|(Content|Destination|Search)DbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Seo must not introduce peer-schema FK/nav or share foreign DbContexts:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void SeoContentGraph_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P26-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P26-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `seo`", text, StringComparison.Ordinal);
        Assert.Contains("SEO != Content editorial", text, StringComparison.Ordinal);
        Assert.Contains("SEO != Destination hierarchy SoR", text, StringComparison.Ordinal);
        Assert.Contains("TC-P26-T004", text, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
