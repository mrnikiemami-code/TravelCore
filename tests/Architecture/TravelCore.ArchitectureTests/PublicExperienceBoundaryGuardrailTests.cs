using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P14-T001 / P14-R1: Public Experience Layer owns Detail/Listing/Landing presentation.
/// Not Search engine. Not Tour catalog. No Booking/Payment.
/// </summary>
public sealed class PublicExperienceBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PublicExperienceContracts_Exist_Without_Persistence_Schema()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Contracts");
        Assert.DoesNotContain(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Infrastructure");
        Assert.DoesNotContain(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Domain");

        Assert.Equal(
            "PublicExperience",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.SurfaceOwnerModule);
        Assert.Equal(
            "Tour",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.CatalogOwnerModule);
        Assert.Equal(
            "Search",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.SearchOwnerModule);
    }

    [Fact]
    public void PublicExperienceContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.PublicExperience.Contracts");
        Assert.Empty(contracts.ProjectReferences);

        var forbidden = new[] { "Tour", "Seo", "Pricing", "Search", "Booking", "Payment", "AgencyMarketplace" };
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name => forbidden.Any(f => name.Contains($".{f}.", StringComparison.OrdinalIgnoreCase)
                                             || name.EndsWith($".{f}", StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(hits.Count == 0, "PublicExperience.Contracts must not project-reference peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PublicExperience_DoesNotOwn_Booking_SearchEngine_Or_CatalogTypes()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "PublicExperience");
        Assert.True(Directory.Exists(root), root);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
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
                        @"\b(class|record|enum|struct|interface)\s+(TourProduct|TourDeparture|Booking|Payment|SearchDocument|IndexPolicy)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Public Experience must not own Tour/Booking/Search/SEO types:\n" + string.Join('\n', hits));

        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Search")));
    }

    [Fact]
    public void Frontend_PublicExperience_Surfaces_Match_Contracts()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "surfaces.ts");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("detail", text, StringComparison.Ordinal);
        Assert.Contains("listing", text, StringComparison.Ordinal);
        Assert.Contains("landing", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", text, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicDetailStickyActions_Are_Not_Booking()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "detail-sticky-actions.tsx");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("View departures", text, StringComparison.Ordinal);
        Assert.Contains("View price", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Pay Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Reserve Seat", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Checkout", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/api/booking", text, StringComparison.Ordinal);
    }
}
