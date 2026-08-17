using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Search.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P15-T001 / P15-R1 and TC-P15-T002 / P15-R2: Search is Discovery owner with a hybrid read-model
/// abstraction. No FTS, ranking, faceting, or physical search engine.
/// </summary>
public sealed class SearchBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void SearchProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Search.Infrastructure");
        Assert.Equal(
            "search",
            TravelCore.Modules.Search.Infrastructure.SearchDbContext.SchemaName);
    }

    [Fact]
    public void Search_DoesNot_Own_Peer_Facts_Or_Engines()
    {
        Assert.Equal("Search", SearchOwnershipBoundary.DiscoveryOwnerModule);
        Assert.False(SearchOwnershipBoundary.OwnsTourFacts);
        Assert.False(SearchOwnershipBoundary.OwnsContentFacts);
        Assert.False(SearchOwnershipBoundary.OwnsPricingFacts);
        Assert.False(SearchOwnershipBoundary.OwnsAgencyFacts);
        Assert.False(SearchOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(SearchOwnershipBoundary.RankingEngineAllowed);
        Assert.False(SearchOwnershipBoundary.FacetingEngineAllowed);
        Assert.False(SearchOwnershipBoundary.FullTextSearchImplemented);
        Assert.False(SearchOwnershipBoundary.ElasticsearchCommitted);
        Assert.False(SearchOwnershipBoundary.RecommendationEngineAllowed);
        Assert.Equal("HybridReadModel", SearchIndexBoundary.ReadModelPosture);
        Assert.False(SearchIndexBoundary.SearchDocumentIsDomainEntity);
        Assert.False(SearchIndexBoundary.PhysicalEngineCommitted);
        Assert.False(SearchIndexBoundary.SqlFullTextCommitted);
        Assert.False(SearchIndexBoundary.OpenSearchCommitted);
        Assert.False(SearchIndexBoundary.ElasticsearchCommitted);
        Assert.False(SearchIndexBoundary.EmbeddingAllowed);
        Assert.False(SearchIndexBoundary.RankingEngineAllowed);
        Assert.False(SearchIndexBoundary.FacetingEngineAllowed);
    }

    [Fact]
    public void SearchInfrastructure_MustNotImplement_ISearchIndex()
    {
        var infraRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Search",
            "TravelCore.Modules.Search.Infrastructure");
        Assert.True(Directory.Exists(infraRoot), infraRoot);

        var hits = Directory.EnumerateFiles(infraRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(x.line, @"\bISearchIndex\b")
                           || Regex.IsMatch(x.line, @"\bDbSet\s*<\s*SearchDocument\s*>");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T002 forbids a concrete index engine and SearchDocument persistence:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Search.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Search.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SearchContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Search.Contracts");
        Assert.Empty(contracts.ProjectReferences);
    }

    [Fact]
    public void SearchModule_Forbids_Engine_And_Fact_Identifiers()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");
        Assert.True(Directory.Exists(root), root);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(pg_trgm|to_tsvector|ts_rank|Elasticsearch|OpenSearch|Booking|Payment|PriceOverride|Commission|SetIndexPolicy|IEmbedding|OpenAI)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Search scaffolding must not implement FTS/engines or steal facts:\n" + string.Join('\n', hits));
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Tour", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Content.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Content", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Pricing", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".AgencyMarketplace.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".AgencyMarketplace", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Seo.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Seo", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
