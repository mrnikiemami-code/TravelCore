using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P07-T008: Place catalog boundary hardening — catalog≠booking; Place↛SEO peer infra;
/// public Place surface does not invent SEO IndexPolicy or Admin lifecycle leaks.
/// </summary>
public sealed class PlaceCatalogBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PlaceInfrastructure_MustNotProjectReference_SeoInfrastructureOrDomain()
    {
        var placeInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Place.Infrastructure");
        var violations = placeInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Equals("TravelCore.Modules.Seo.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Place.Infrastructure must not depend on SEO.Infrastructure/Domain (SEO owns route/history/IndexPolicy):\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void PlaceDomain_MustNotProjectReference_SeoOrPeerBusinessInfrastructure()
    {
        var placeDomain = Projects.Single(p => p.Name == "TravelCore.Modules.Place.Domain");
        var forbidden = placeDomain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Domain", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Contracts", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Destination.Domain", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Media.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "Place.Domain must stay free of peer Infrastructure/Domain and SEO contracts:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void PlaceModule_ForbidsHotelBookingProductSignals()
    {
        var placeRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Place");
        Assert.True(Directory.Exists(placeRoot), placeRoot);

        // Comments may mention HotelBooking as the owning future module; forbid product fields/APIs only.
        var hits = Directory.EnumerateFiles(placeRoot, "*.cs", SearchOption.AllDirectories)
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
                        @"\b(HotelBooking|ReservationId|InventoryId|LiveInventory|RoomRate|VoucherCode|BookingEngine|AvailabilityCalendar|RatePlanId|Allotment)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Hotel Catalog ≠ Hotel Booking — Place must not ship booking/inventory product signals:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PlaceAggregate_HasNoGlobalSlug_PlaceTranslationOwnsCurrentSlug()
    {
        var placePath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Place",
            "TravelCore.Modules.Place.Domain",
            "Place.cs");
        var translationPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Place",
            "TravelCore.Modules.Place.Domain",
            "PlaceTranslation.cs");

        Assert.True(File.Exists(placePath), placePath);
        Assert.True(File.Exists(translationPath), translationPath);

        var placeText = File.ReadAllText(placePath);
        var translationText = File.ReadAllText(translationPath);

        Assert.DoesNotMatch(new Regex(@"\bpublic\s+string\??\s+Slug\b", RegexOptions.CultureInvariant), placeText);
        Assert.DoesNotContain("SlugFa", placeText, StringComparison.Ordinal);
        Assert.DoesNotContain("SlugEn", placeText, StringComparison.Ordinal);
        Assert.Contains("public string? Slug", translationText, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicPlaceDetail_UsesAppProxyMedia_AndDoesNotLeakAdminLifecycle()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "place-detail");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var hits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
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
                        @"\b(storageKey|DeletedAt|ArchivedAt|IsDeleted|SetIndexPolicy|admin/catalog/places)\b",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Public Place detail must not leak StorageKey / Admin lifecycle / IndexPolicy mutation:\n"
            + string.Join('\n', hits));

        var loaderPath = Path.Combine(featureRoot, "load-place-detail.ts");
        Assert.True(File.Exists(loaderPath), loaderPath);
        var loader = File.ReadAllText(loaderPath);
        Assert.Contains("resolveMediaAppProxySrc", loader, StringComparison.Ordinal);
        Assert.Contains("mediaOriginalContentPath", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", loader, StringComparison.Ordinal);

        var viewPath = Path.Combine(featureRoot, "place-detail-view.tsx");
        Assert.True(File.Exists(viewPath), viewPath);
        Assert.DoesNotMatch(new Regex(@"\bHero\b"), File.ReadAllText(viewPath));
    }
}
