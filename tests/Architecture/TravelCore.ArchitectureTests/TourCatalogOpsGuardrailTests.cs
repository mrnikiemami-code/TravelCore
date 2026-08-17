using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T008 / P09-R4/R6: Tour catalog publication status closed set; Published ≠ Index.
/// </summary>
public sealed class TourCatalogOpsGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourCatalogStatus_IsClosedDraftPublishedInactive()
    {
        Assert.Equal(
            [TourCatalogStatus.Draft, TourCatalogStatus.Published, TourCatalogStatus.Inactive],
            Enum.GetValues<TourCatalogStatus>());
    }

    [Fact]
    public void TourCatalogOps_ForbidDeleteArchiveBookableNowTypes()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && !x.line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(class|record|enum|struct)\s+(TourDeleted|TourArchived|BookableNow|TourHardDelete)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour must not invent delete/archive/bookable-now product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void SeoTourProductPublication_DoesNot_SetIndexPolicy()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Seo",
            "TravelCore.Modules.Seo.Infrastructure",
            "Services",
            "SeoTourProductPublicationService.cs");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.DoesNotContain("ISeoIndexPolicyService", text, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        Assert.Contains("tours/", text, StringComparison.Ordinal);
    }
}
