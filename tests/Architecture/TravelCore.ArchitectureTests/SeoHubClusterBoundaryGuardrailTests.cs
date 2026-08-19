using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P26-T005 / P26-R2: hub/cluster taxonomy boundary without editorial duplication or persistence.
/// </summary>
public sealed class SeoHubClusterBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void SeoDomain_Exposes_HubClusterBoundary_Models()
    {
        Assert.NotNull(typeof(SeoHubClusterKind));
        Assert.NotNull(typeof(SeoHubClusterBoundary));
        Assert.NotNull(typeof(SeoHubClusterReference));
        Assert.True(SeoContentGraphOwnershipBoundary.HubClusterBoundaryImplemented);
        Assert.False(SeoHubClusterBoundary.HubClusterPersistenceImplemented);
    }

    [Fact]
    public void SeoHubClusterBoundary_Keeps_Editorial_And_Hierarchy_SoR_Out()
    {
        Assert.Equal("DestinationHub · ContentCluster", SeoHubClusterBoundary.HubClusterTaxonomy);
        Assert.Equal("Seo", SeoHubClusterBoundary.GraphMechanicsOwner);
        Assert.Equal("Content", SeoHubClusterBoundary.ContentPublisherOwner);
        Assert.Equal("Destination", SeoHubClusterBoundary.DestinationPublisherOwner);
        Assert.True(SeoHubClusterBoundary.SeoOwnsHubClusterTaxonomy);
        Assert.False(SeoHubClusterBoundary.HubEditorialDuplicationImplemented);
        Assert.False(SeoHubClusterBoundary.DestinationHierarchySoRImplemented);
    }

    [Fact]
    public void SeoHubClusterKind_Lists_Planned_Taxonomy_Only()
    {
        var names = Enum.GetNames<SeoHubClusterKind>();
        Assert.Equal(2, names.Length);
        Assert.Contains("DestinationHub", names);
        Assert.Contains("ContentCluster", names);
    }

    [Fact]
    public void Seo_T005_Forbids_Hub_Persistence_And_Editorial_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Seo");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(SeoHubContent|SeoClusterArticle|SeoHubEditorialBody|SeoDestinationHierarchy|IContentHubRepository|SeoHubTable)\b",
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

        Assert.True(hits.Count == 0, "Seo T005 forbids hub editorial/persistence product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Seo_T005_Has_No_New_Migration_Or_Hub_Table_Additions()
    {
        var migrationsDir = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Seo",
            "TravelCore.Modules.Seo.Infrastructure",
            "Migrations");
        var migrationFiles = Directory.Exists(migrationsDir)
            ? Directory.GetFiles(migrationsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)
                    && !f.Contains("AddSeoContentGraphFoundation", StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Contains("Hub", StringComparison.OrdinalIgnoreCase)
                    || f.Contains("Cluster", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];
        Assert.Empty(migrationFiles);
        Assert.False(SeoHubClusterBoundary.HubClusterPersistenceImplemented);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
