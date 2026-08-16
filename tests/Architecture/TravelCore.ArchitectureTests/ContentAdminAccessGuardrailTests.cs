using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P08-T007: Access-backed Admin Content baseline — no inventing R3/R4/R6/R7/R8.
/// </summary>
public sealed class ContentAdminAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentEndpoints_Mutations_Require_ContentItemsWrite_Policy()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Infrastructure",
            "Endpoints",
            "ContentEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.Content.Items.Write", text, StringComparison.Ordinal);

        // No ContentItem delete/archive HTTP surface (P08-R8 open).
        Assert.DoesNotMatch(
            new Regex(@"MapDelete\s*\(\s*""/\{id:guid\}""\s*,", RegexOptions.CultureInvariant),
            text);
        Assert.DoesNotContain("DeletedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ArchivedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("IsDeleted", text, StringComparison.Ordinal);

        // Slug / IndexPolicy remain unresolved (P08-R3/R4) — Admin baseline must not invent.
        Assert.DoesNotContain("by-slug", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ISeoIndexPolicy", text, StringComparison.Ordinal);
        Assert.DoesNotContain("AuthorId", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ContentAuthor", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TourWidget", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HotelWidget", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AttractionWidget", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AccessCatalog_Includes_ContentItemsWrite()
    {
        var catalogPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Access",
            "TravelCore.Modules.Access.Domain",
            "AccessPermissionCatalog.cs");
        var text = File.ReadAllText(catalogPath);
        Assert.Contains("content.items.write", text, StringComparison.Ordinal);

        var policiesPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Access",
            "TravelCore.Modules.Access.Infrastructure",
            "Authorization",
            "AccessAuthorizationPolicies.cs");
        var policies = File.ReadAllText(policiesPath);
        Assert.Contains("Access.Content.Items.Write", policies, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminContentFrontend_Omits_DeleteArchiveSlugSeoAuthorAndWidgets()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-content");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var hits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(featureRoot, "*.tsx", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(deleteContent|archiveContent|restoreContent|IsDeleted|DeletedAt|ArchivedAt|SetIndexPolicy|ISeoIndexPolicy|Author|Widget|TourWidget|HotelWidget)\b",
                                RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Admin Content must not invent R8 delete/archive, R3/R4 SEO/slug, R7 Author, or R6 widgets:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AdminContentFrontend_Uses_ReadyMediaPicker_For_ImageBlocks()
    {
        var islandPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "admin-content",
            "content-workflow-island.tsx");
        Assert.True(File.Exists(islandPath), islandPath);
        var text = File.ReadAllText(islandPath);

        Assert.Contains("listMediaAssetsAction", text, StringComparison.Ordinal);
        Assert.Contains("status: \"Ready\"", text, StringComparison.Ordinal);
        Assert.Contains("addImageBlock", text, StringComparison.Ordinal);
        Assert.Contains("mediaVariantContentPath", text, StringComparison.Ordinal);
        Assert.DoesNotContain("setImageMediaId", text, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", text, StringComparison.Ordinal);
    }
}
