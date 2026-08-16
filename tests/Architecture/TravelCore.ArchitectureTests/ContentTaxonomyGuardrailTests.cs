using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// Content taxonomy guardrails (TC-P08-T004) — Category/Tag without inventing Author (P08-R7).
/// </summary>
public sealed class ContentTaxonomyGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentTaxonomy_ExistsWithoutAuthorUntilP08R7()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        Assert.True(Directory.Exists(contentRoot), contentRoot);

        var categoryPath = Path.Combine(
            contentRoot,
            "TravelCore.Modules.Content.Domain",
            "ContentCategory.cs");
        var tagPath = Path.Combine(
            contentRoot,
            "TravelCore.Modules.Content.Domain",
            "ContentTag.cs");
        Assert.True(File.Exists(categoryPath), categoryPath);
        Assert.True(File.Exists(tagPath), tagPath);

        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(x.line, @"\b(class\s+ContentAuthor|ContentAuthorId|IContentAuthorService)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content must not invent Author model while P08-R7 is unresolved:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void ContentTaxonomy_DoesNotMergePartyOrIdentity()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(TravelCore\.Modules\.Party|TravelCore\.Modules\.Identity|PartyId|IdentityUserId)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content taxonomy must not merge Party/Identity:\n" + string.Join('\n', hits));
    }
}
