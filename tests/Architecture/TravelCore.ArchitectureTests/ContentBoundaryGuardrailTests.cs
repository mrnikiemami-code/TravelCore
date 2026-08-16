using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P08-T009: Content boundary hardening — schema isolation; Content≠SEO substance;
/// Content≠Tour/Place ownership; R6–R8 remain uninvented; public default noindex,follow.
/// </summary>
public sealed class ContentBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void ContentInfrastructure_MustNotProjectReference_SeoInfrastructureOrDomain()
    {
        var contentInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Content.Infrastructure");
        var violations = contentInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Equals("TravelCore.Modules.Seo.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Content.Infrastructure must not depend on SEO.Infrastructure/Domain (SEO owns route/history/IndexPolicy):\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void ContentDomain_MustNotProjectReference_SeoOrPeerBusinessInfrastructure()
    {
        var contentDomain = Projects.Single(p => p.Name == "TravelCore.Modules.Content.Domain");
        var forbidden = contentDomain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Domain", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Contracts", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Destination.Domain", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Place.Domain", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Media.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "Content.Domain must stay free of peer Infrastructure/Domain and SEO contracts:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void ContentModule_ForbidsTourPlaceOwnershipAndP09ProductSignals()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        Assert.True(Directory.Exists(contentRoot), contentRoot);

        // Comments may mention Tour/Place as referenced peers; forbid ownership/product APIs only.
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(TourProduct|TourDeparture|HotelBooking|Travelogue|UserGeneratedContent|TourWidget|HotelWidget|AttractionWidget|ITourService|PlaceAggregate|class\s+Place\b)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content ≠ Tour/Place ownership — no Tour widgets / P09 product / Place aggregate ownership:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void ContentModule_ForbidsCrossSchemaFkWrites_ToDestinationPlaceSeoMedia()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(destination|place|seo|media)""|HasOne<.*(Destination|Place|Seo|Media)|TravelCore\.Modules\.(Destination|Place|Seo|Media)\.Domain")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content schema must not introduce cross-schema FK/nav to Destination/Place/SEO/Media:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void ContentDoesNotOwnIndexPolicy_SeoDoesNotOwnContentBody()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var contentHits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(SeoIndexPolicy|SetIndexPolicy|ISeoIndexPolicyService|IndexPolicyId|class\s+IndexPolicy)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            contentHits.Count == 0,
            "Content must not own IndexPolicy (P08-R4 — SEO owns indexability):\n"
            + string.Join('\n', contentHits));

        // SEO publication for Content binds routes only — no editorial body/title persistence.
        var seoServices = Path.Combine(
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
            var path = Path.Combine(seoServices, fileName);
            Assert.True(File.Exists(path), path);
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("localizedBody", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ContentItemTranslation", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
            Assert.DoesNotContain("TravelCore.Modules.Content.Domain", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ContentAggregate_HasNoGlobalSlug_TranslationOwnsCurrentSlug()
    {
        var itemPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Domain",
            "ContentItem.cs");
        var translationPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Domain",
            "ContentItemTranslation.cs");

        Assert.True(File.Exists(itemPath), itemPath);
        Assert.True(File.Exists(translationPath), translationPath);

        var itemText = File.ReadAllText(itemPath);
        var translationText = File.ReadAllText(translationPath);

        Assert.DoesNotMatch(new Regex(@"\bpublic\s+string\??\s+Slug\b", RegexOptions.CultureInvariant), itemText);
        Assert.DoesNotContain("SlugFa", itemText, StringComparison.Ordinal);
        Assert.DoesNotContain("SlugEn", itemText, StringComparison.Ordinal);
        Assert.Contains("public string? Slug", translationText, StringComparison.Ordinal);
    }

    [Fact]
    public void UnresolvedR6R7R8_RemainUninvented_InContentProductSurfaces()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class\s+ContentAuthor|ContentAuthorId|TourWidget|HotelWidget|AttractionWidget|DeletedAt|ArchivedAt|IsDeleted|HardDelete|ArchiveAsync)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "P08-R6/R7/R8 must remain UNRESOLVED — no Author / widgets / delete-archive product:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PublicContentPages_DefaultNoindexFollow_AndDoNotLeakAdminLifecycle()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "content-detail");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var featureHits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(featureRoot, "*.tsx", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal)
                        || trimmed.StartsWith("*", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(DeletedAt|ArchivedAt|IsDeleted|SetIndexPolicy|admin/catalog/content|TourWidget|AuthorId)\b",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            featureHits.Count == 0,
            "Public Content detail must not leak Admin lifecycle / IndexPolicy mutation / R6–R7:\n"
            + string.Join('\n', featureHits));

        foreach (var relative in new[]
                 {
                     Path.Combine("src", "frontend", "web", "src", "app", "[locale]", "articles", "[slug]", "page.tsx"),
                     Path.Combine("src", "frontend", "web", "src", "app", "[locale]", "landing-pages", "[slug]", "page.tsx")
                 })
        {
            var pagePath = Path.Combine(RepoRoot, relative);
            Assert.True(File.Exists(pagePath), pagePath);
            var text = File.ReadAllText(pagePath);
            Assert.Contains("P08-R4", text, StringComparison.Ordinal);
            Assert.Contains("index: false, follow: true", text, StringComparison.Ordinal);
            Assert.Contains("robotsFromComposed", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        }
    }
}
