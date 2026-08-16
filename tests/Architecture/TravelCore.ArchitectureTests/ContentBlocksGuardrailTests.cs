using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// Content Blocks guardrails (TC-P08-T005 / P08-R2 relational; no P08-R6 widgets).
/// </summary>
public sealed class ContentBlocksGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentBlocks_AreRelationalWithoutJsonbDefaultAndWithoutWidgets()
    {
        var blockPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Domain",
            "ContentBlock.cs");
        Assert.True(File.Exists(blockPath), blockPath);
        var text = File.ReadAllText(blockPath);
        Assert.Contains("ContentBlockKind", text, StringComparison.Ordinal);
        Assert.Contains("SortOrder", text, StringComparison.Ordinal);

        var kindPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Domain",
            "ContentBlockKind.cs");
        var kindText = File.ReadAllText(kindPath);
        Assert.Contains("Heading", kindText, StringComparison.Ordinal);
        Assert.DoesNotContain("TourWidget", kindText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HotelWidget", kindText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AttractionWidget", kindText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ContentBlocks_ForbidCrossSchemaMediaFkInSource()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"HasOne<.*Media|TravelCore\.Modules\.Media\.Domain|principalSchema:\s*""media""")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content blocks must not introduce Media schema FK:\n" + string.Join('\n', hits));
    }
}
