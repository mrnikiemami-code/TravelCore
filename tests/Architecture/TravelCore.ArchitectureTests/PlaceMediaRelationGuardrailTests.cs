using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Place.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P07-T005: Place owns Place↔Media relationship meaning; Media.Contracts only.
/// </summary>
public sealed class PlaceMediaRelationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PlaceMediaRole_IsClosedCoverGalleryOnly()
    {
        Assert.Equal(
            [PlaceMediaRole.Cover, PlaceMediaRole.Gallery],
            Enum.GetValues<PlaceMediaRole>());
    }

    [Fact]
    public void PlaceInfrastructure_MustNotProjectReference_MediaInfrastructureOrDomain()
    {
        var placeInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Place.Infrastructure");
        var violations = placeInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Equals("TravelCore.Modules.Media.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Media.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Place.Infrastructure must depend on Media.Contracts only:\n" + string.Join('\n', violations));

        Assert.Contains(
            placeInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name.Equals("TravelCore.Modules.Media.Contracts", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PlaceModule_ForbidsPersistedStorageKeysOrUrlsAndCrossSchemaMediaFk()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && !x.line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                            && Regex.IsMatch(
                    x.line,
                    @"\b(StorageKey|presigned|principalSchema:\s*""media""|MediaDbContext|HasOne<\s*MediaAsset)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Place must not persist StorageKey/URLs or introduce Media EF/cross-schema FK:\n"
            + string.Join('\n', hits));
    }
}
