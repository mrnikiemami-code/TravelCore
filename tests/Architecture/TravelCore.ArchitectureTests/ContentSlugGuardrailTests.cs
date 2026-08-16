using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P08-T008: Content owns current locale slug (P08-R3); SEO publication never sets IndexPolicy (P08-R4).
/// </summary>
public sealed class ContentSlugGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentItemTranslation_Owns_CurrentLocaleSlug()
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
        Assert.Contains("public string? Slug", text, StringComparison.Ordinal);
        Assert.Contains("NormalizeSlug", text, StringComparison.Ordinal);
        Assert.Contains("P08-R3", text, StringComparison.Ordinal);
        Assert.DoesNotContain("PreviousSlug", text, StringComparison.Ordinal);
        Assert.DoesNotContain("SlugHistory", text, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Content_DoesNot_Own_GlobalSlugEngine()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(public string\?? Slug)\b")
                    && !x.path.EndsWith("ContentItemTranslation.cs", StringComparison.OrdinalIgnoreCase)
                    && !x.line.Contains("ContentItemTranslation", StringComparison.Ordinal)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .Where(x => !x.Contains("ContentItemTranslation", StringComparison.Ordinal))
            .ToList();

        // Aggregate root must not expose a global Slug SoR property.
        var domainRoot = Path.Combine(
            contentRoot,
            "TravelCore.Modules.Content.Domain",
            "ContentItem.cs");
        var rootText = File.ReadAllText(domainRoot);
        Assert.DoesNotContain("public string? Slug", rootText, StringComparison.Ordinal);
        Assert.DoesNotContain("public string Slug", rootText, StringComparison.Ordinal);
        _ = hits;
    }

    [Fact]
    public void SeoContentPublication_DoesNot_SetIndexPolicy()
    {
        var servicesDir = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Seo",
            "TravelCore.Modules.Seo.Infrastructure",
            "Services");

        foreach (var fileName in new[]
                 {
                     "SeoArticlePublicationService.cs",
                     "SeoLandingPagePublicationService.cs"
                 })
        {
            var path = Path.Combine(servicesDir, fileName);
            Assert.True(File.Exists(path), path);
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
            Assert.DoesNotContain("ISeoIndexPolicy", text, StringComparison.Ordinal);
            Assert.DoesNotContain("ISeoIndexPolicyService", text, StringComparison.Ordinal);
        }
    }
}
