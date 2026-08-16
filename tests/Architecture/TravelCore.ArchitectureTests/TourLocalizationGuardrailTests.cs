using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// Tour localization guardrails (TC-P09-T003 / ADR 0008).
/// </summary>
public sealed class TourLocalizationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourSchema_ForbidsTitleFaTitleEnColumnsInSource()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(TitleFa|TitleEn|NameFa|NameEn|DescriptionFa|DescriptionEn|title_fa|title_en|name_fa|name_en)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour must not introduce TitleFa/TitleEn-style columns:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void TourProductTranslation_HasLocaleRowsWithoutSlug_UntilP09R5()
    {
        var translationPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Domain",
            "TourProductTranslation.cs");
        Assert.True(File.Exists(translationPath), translationPath);

        var text = File.ReadAllText(translationPath);
        Assert.Contains("public string Title", text, StringComparison.Ordinal);
        Assert.Contains("public string? Description", text, StringComparison.Ordinal);
        Assert.DoesNotContain("public string? Slug", text, StringComparison.Ordinal);
        Assert.Contains("P09-R5", text, StringComparison.Ordinal);
    }
}
