using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T007 / P09-R8: Tour↔Media Cover/Gallery logical links only.
/// </summary>
public sealed class TourMediaRelationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void TourMediaRole_IsClosedCoverGalleryOnly()
    {
        var names = Enum.GetNames<TourMediaRole>();
        Assert.Equal(["Cover", "Gallery"], names);
    }

    [Fact]
    public void IMediaAssetReadinessQuery_IsContractsSurface()
    {
        Assert.Equal(
            "TravelCore.Modules.Media.Contracts",
            typeof(IMediaAssetReadinessQuery).Namespace);
        Assert.NotNull(typeof(IMediaAssetReadinessQuery).GetMethod(nameof(IMediaAssetReadinessQuery.IsReadyAsync)));
    }

    [Fact]
    public void TourInfrastructure_MustReference_MediaContracts_Only()
    {
        var tourInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Tour.Infrastructure");
        var refs = tourInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .ToList();

        Assert.Contains(
            refs,
            name => name.Equals("TravelCore.Modules.Media.Contracts", StringComparison.OrdinalIgnoreCase));

        var violations = refs
            .Where(name =>
                name.Equals("TravelCore.Modules.Media.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Media.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Tour.Infrastructure must depend on Media.Contracts only:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void TourMediaLinks_ForbidStorageKeyAndCrossSchemaFk()
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
                    @"\b(StorageKey|presigned|MediaDbContext|HasOne<\s*MediaAsset|principalSchema:\s*""media""|TravelCore\.Modules\.Media\.Domain)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour must not store StorageKey or introduce Media schema FK:\n" + string.Join('\n', hits));
    }
}
