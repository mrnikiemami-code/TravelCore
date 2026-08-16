using System.Reflection;
using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Media.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P06-T010 / P06-R5: contract-only MediaAssetId consumer reference proof (no Destination schema).
/// </summary>
public sealed class MediaConsumerReferenceGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void MediaAssetReference_IsStableIdentity_WithoutStorageOrProviderSurface()
    {
        var id = Guid.Parse("01900000-0000-7000-8000-000000000001");
        var reference = MediaAssetReference.From(id);

        Assert.Equal(id, reference.MediaAssetId);
        Assert.Equal(id.ToString("D"), reference.ToString());

        var props = typeof(MediaAssetReference).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(props, p => p.Name == nameof(MediaAssetReference.MediaAssetId));
        Assert.DoesNotContain(props, p => p.Name.Contains("Storage", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Name.Contains("Url", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Name.Contains("Path", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Name.Contains("Key", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(
            "TravelCore.Modules.Media.Contracts",
            MediaConsumerReferenceBoundary.AllowedConsumerDependency);
        Assert.Contains("app proxy", MediaConsumerReferenceBoundary.PresentationBoundary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MediaAssetReference_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => MediaAssetReference.From(Guid.Empty));
    }

    [Fact]
    public void PeerModules_MustNotProjectReference_MediaInfrastructure()
    {
        var peers = Projects
            .Where(p => p.IsUnderSrc)
            .Where(p =>
            {
                var relative = p.RelativePath.Replace('\\', '/');
                return relative.Contains("/Modules/", StringComparison.OrdinalIgnoreCase)
                       && !p.Name.StartsWith("TravelCore.Modules.Media", StringComparison.Ordinal);
            })
            .ToList();

        Assert.False(peers.Count == 0, "Expected non-Media module projects.");

        var violations = new List<string>();
        foreach (var project in peers)
        {
            foreach (var reference in project.ProjectReferences)
            {
                var name = Path.GetFileNameWithoutExtension(reference);
                if (name.Equals("TravelCore.Modules.Media.Infrastructure", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("TravelCore.Modules.Media.Domain", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{project.Name} -> {name}");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Peer modules must depend on Media.Contracts only (not Media.Infrastructure/Domain):\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void MediaModule_HasNoGenericConsumerRelationshipTablesOrEntities()
    {
        var mediaRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Media");
        Assert.True(Directory.Exists(mediaRoot), mediaRoot);

        // Explicitly forbid known table/entity type declarations.
        var typeDecl = Directory.EnumerateFiles(mediaRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(class|record|struct)\s+(MediaAssetLink|MediaConsumerLink|MediaAttachment|MediaConsumerReference)\b",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            typeDecl.Count == 0,
            "Media must not introduce generic consumer relationship entities:\n" + string.Join('\n', typeDecl));
    }

    [Fact]
    public void Destination_HasNoMediaAssetIdPersistenceField()
    {
        var destinationRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Destination");
        Assert.True(Directory.Exists(destinationRoot), destinationRoot);

        var hits = Directory.EnumerateFiles(destinationRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(MediaAssetId|HeroMediaAssetId|CoverMediaAssetId|ThumbnailMediaAssetId)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "P06-R5 CONTRACT-ONLY forbids Destination MediaAssetId fields:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void MediaPresentation_IsSeparateFromConsumerReferenceContract()
    {
        Assert.NotEqual(
            typeof(MediaAssetReference).FullName,
            typeof(MediaAssetPresentationResponse).FullName);
        Assert.NotNull(typeof(MediaPresentationUrls).GetMethod(nameof(MediaPresentationUrls.OriginalContent)));
        Assert.DoesNotContain(
            typeof(MediaAssetReference).GetProperties(),
            p => p.Name.Contains("ContentUrl", StringComparison.OrdinalIgnoreCase)
                 || p.Name.Contains("Presentation", StringComparison.OrdinalIgnoreCase));
    }
}
