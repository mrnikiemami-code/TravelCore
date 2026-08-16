using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// Content localization guardrails (TC-P08-T003 / ADR 0008).
/// </summary>
public sealed class ContentLocalizationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentSchema_ForbidsTitleFaTitleEnBodyFaColumnsInSource()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(TitleFa|TitleEn|BodyFa|BodyEn|ExcerptFa|ExcerptEn|title_fa|title_en|body_fa|body_en)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content must not introduce TitleFa/TitleEn/BodyFa-style columns:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void ContentItemTranslation_ExistsWithoutSlugUntilP08R3()
    {
        var translationPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Domain",
            "ContentItemTranslation.cs");
        Assert.True(File.Exists(translationPath), translationPath);

        var text = File.ReadAllText(translationPath);
        Assert.Contains("public string Title", text, StringComparison.Ordinal);
        Assert.Contains("public string? Body", text, StringComparison.Ordinal);
        Assert.Contains("public string? Excerpt", text, StringComparison.Ordinal);
        // P08-R3 UNRESOLVED — do not invent Content-owned slug yet.
        Assert.DoesNotContain("public string? Slug", text, StringComparison.Ordinal);
        Assert.DoesNotContain("NormalizeSlug", text, StringComparison.Ordinal);
    }
}
