using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P07-T006: Access-backed Admin Place baseline — no inventing R3/R4/R5.
/// </summary>
public sealed class PlaceAdminAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PlaceEndpoints_Mutations_Require_PlacePlacesWrite_Policy()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Place",
            "TravelCore.Modules.Place.Infrastructure",
            "Endpoints",
            "PlaceEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.Place.Places.Write", text, StringComparison.Ordinal);

        // No Place delete/archive HTTP surface (P07-R3).
        Assert.DoesNotMatch(
            new Regex(@"MapDelete\s*\(\s*""/\{id:guid\}""\s*,", RegexOptions.CultureInvariant),
            text);
        Assert.DoesNotContain("DeletedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ArchivedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("IsDeleted", text, StringComparison.Ordinal);

        // No slug / SEO index routes (P07-R4 / P07-R5).
        Assert.DoesNotContain("/slug", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IndexPolicy", text, StringComparison.Ordinal);
        Assert.DoesNotContain("by-slug", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AccessCatalog_Includes_PlacePlacesWrite()
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
        Assert.Contains("place.places.write", text, StringComparison.Ordinal);

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
        Assert.Contains("Access.Place.Places.Write", policies, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminPlaceFrontend_Omits_DeleteArchiveSlugAndSeoControls()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-place");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var hits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(featureRoot, "*.tsx", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(deletePlace|archivePlace|restorePlace|IsDeleted|DeletedAt|ArchivedAt|PlaceTranslation\.Slug|IndexPolicy)\b",
                                RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Admin Place must not invent R3/R4/R5 surfaces:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AdminPlaceFrontend_Uses_ReadyMediaPicker_Not_RawIdPaste()
    {
        var islandPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "admin-place",
            "place-workflow-island.tsx");
        Assert.True(File.Exists(islandPath), islandPath);
        var text = File.ReadAllText(islandPath);

        Assert.Contains("listMediaAssetsAction", text, StringComparison.Ordinal);
        Assert.Contains("status: \"Ready\"", text, StringComparison.Ordinal);
        Assert.Contains("useAsCover", text, StringComparison.Ordinal);
        Assert.Contains("addToGallery", text, StringComparison.Ordinal);
        Assert.Contains("mediaVariantContentPath", text, StringComparison.Ordinal);
        Assert.DoesNotContain("setCoverMediaId", text, StringComparison.Ordinal);
        Assert.DoesNotContain("setGalleryMediaId", text, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", text, StringComparison.Ordinal);
    }
}
