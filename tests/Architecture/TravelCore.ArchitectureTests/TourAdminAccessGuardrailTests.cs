using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T009: Access-backed Admin Tour baseline — no delete/archive invent; no IndexPolicy in Tour.
/// </summary>
public sealed class TourAdminAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourEndpoints_Mutations_Require_TourProductsWrite_Policy()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Endpoints",
            "TourEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.Tour.Products.Write", text, StringComparison.Ordinal);

        Assert.DoesNotMatch(
            new Regex(@"MapDelete\s*\(\s*""/\{id:guid\}""\s*,", RegexOptions.CultureInvariant),
            text);
        Assert.DoesNotContain("DeletedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ArchivedAt", text, StringComparison.Ordinal);
        Assert.DoesNotContain("IsDeleted", text, StringComparison.Ordinal);

        Assert.Contains("by-slug", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/slug", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("catalog-status", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ISeoIndexPolicy", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TourDeparture", text, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AccessCatalog_Includes_TourProductsWrite()
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
        Assert.Contains("tour.products.write", text, StringComparison.Ordinal);
        Assert.Contains("seo.tour-posture.write", text, StringComparison.Ordinal);

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
        Assert.Contains("Access.Tour.Products.Write", policies, StringComparison.Ordinal);
        Assert.Contains("Access.Seo.TourPosture.Write", policies, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminTourFrontend_Omits_DeleteArchiveDepartureAndIndexPolicy()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-tour");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var hits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(featureRoot, "*.tsx", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(deleteTour|archiveTour|restoreTour|IsDeleted|DeletedAt|ArchivedAt|SetIndexPolicy|ISeoIndexPolicy|TourDeparture|BookableNow|FlightSegment)\b",
                                RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Admin Tour must not invent delete/archive, IndexPolicy mutation, or Departure/Flight product:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void AdminTourFrontend_Uses_ReadyMediaPicker()
    {
        var islandPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "admin-tour",
            "tour-workflow-island.tsx");
        Assert.True(File.Exists(islandPath), islandPath);
        var text = File.ReadAllText(islandPath);

        Assert.Contains("listMediaAssetsAction", text, StringComparison.Ordinal);
        Assert.Contains("status: \"Ready\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("rawMediaAssetId", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pasteMediaId", text, StringComparison.OrdinalIgnoreCase);
    }
}
