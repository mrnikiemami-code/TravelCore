using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Place.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P07-T004: catalog ops status/facilities/classification — not R3 delete/archive, not bookable-now.
/// </summary>
public sealed class PlaceCatalogOpsGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PlaceCatalogStatus_IsClosedDraftActiveInactive_Only()
    {
        var values = Enum.GetValues<PlaceCatalogStatus>();
        Assert.Equal(
            [PlaceCatalogStatus.Draft, PlaceCatalogStatus.Active, PlaceCatalogStatus.Inactive],
            values);
    }

    [Fact]
    public void PlaceModule_ForbidsDeleteArchiveAndBookableNowSignals()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        Assert.True(Directory.Exists(placeRoot), placeRoot);

        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(IsDeleted|DeletedAt|ArchivedAt|SoftDeleted|PlaceCatalogStatus\.(Deleted|Archived|Retired|Published)|BookableNow|IsBookable|AvailabilityStatus|ReservationId)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T004 must not invent R3 delete/archive or bookable-now signals:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PlaceModule_ForbidsPlaceTranslationSlug_UntilR4()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => x.path.Contains("PlaceTranslation", StringComparison.OrdinalIgnoreCase)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(public|private|internal|protected)\s+.*\bSlug\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "P07-R4 unresolved — PlaceTranslation must not gain Slug in T004:\n" + string.Join('\n', hits));
    }
}
