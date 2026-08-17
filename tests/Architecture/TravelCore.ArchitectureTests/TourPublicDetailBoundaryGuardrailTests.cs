using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T010: Public Tour detail hardening — P09-R6 SEO default, app-proxy media,
/// no StorageKey / SetIndexPolicy / TourDeparture / Booking on the public surface.
/// </summary>
public sealed class TourPublicDetailBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void TourInfrastructure_MustNotProjectReference_SeoInfrastructureOrDomain()
    {
        var tourInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Tour.Infrastructure");
        var violations = tourInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Equals("TravelCore.Modules.Seo.Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.Equals("TravelCore.Modules.Seo.Domain", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Tour.Infrastructure must not depend on SEO.Infrastructure/Domain (SEO owns IndexPolicy):\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void TourModule_ForbidsTourDepartureBookingPricingSearchSignals()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        Assert.True(Directory.Exists(tourRoot), tourRoot);

        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
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
                        @"\b(class|record|enum|struct|interface)\s+(FlightSegment|TourHotelOption|BookingEngine|PriceQuote|SearchIndex)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(ITourDepartureService|IBookingService|IPricingService|ITourSearchService)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour public/product surface forbids Booking/Pricing/Search and later P11 types (Flight/HotelOption); TourDeparture scaffolding is P11-owned:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PublicTourDetail_UsesAppProxyMedia_P09R6_AndDoesNotLeakBoundaries()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "tour-detail");
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
                        @"\b(storageKey|DeletedAt|ArchivedAt|IsDeleted|SetIndexPolicy|admin/catalog/tours|TourDeparture|BookingEngine|PriceQuote)\b",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            featureHits.Count == 0,
            "Public Tour detail must not leak StorageKey / Admin lifecycle / IndexPolicy / Departure/Booking:\n"
            + string.Join('\n', featureHits));

        var loaderPath = Path.Combine(featureRoot, "load-tour-detail.ts");
        Assert.True(File.Exists(loaderPath), loaderPath);
        var loader = File.ReadAllText(loaderPath);
        Assert.Contains("media/presentation", loader, StringComparison.Ordinal);
        Assert.Contains("resolveMediaAppProxySrc", loader, StringComparison.Ordinal);
        Assert.Contains("mediaOriginalContentPath", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", loader, StringComparison.Ordinal);

        var viewPath = Path.Combine(featureRoot, "tour-detail-view.tsx");
        Assert.True(File.Exists(viewPath), viewPath);
        Assert.DoesNotMatch(new Regex(@"\bHero\b"), File.ReadAllText(viewPath));

        var pagePath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "tours",
            "[slug]",
            "page.tsx");
        Assert.True(File.Exists(pagePath), pagePath);
        var page = File.ReadAllText(pagePath);
        Assert.Contains("P09-R6", page, StringComparison.Ordinal);
        Assert.Contains("index: false, follow: true", page, StringComparison.Ordinal);
        Assert.Contains("robotsFromComposed", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", page, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", page, StringComparison.Ordinal);
        Assert.DoesNotContain("TourDeparture", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking", page, StringComparison.Ordinal);
    }

    [Fact]
    public void TourMediaPresentationEndpoint_ComposesViaMediaContracts_NoStorageKey()
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
        var endpoints = File.ReadAllText(endpointsPath);
        Assert.Contains("media/presentation", endpoints, StringComparison.Ordinal);
        Assert.Contains("GetMediaPresentationAsync", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", endpoints, StringComparison.Ordinal);

        var servicePath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Services",
            "TourProductMediaService.cs");
        Assert.True(File.Exists(servicePath), servicePath);
        var service = File.ReadAllText(servicePath);
        Assert.Contains("IMediaPresentationService", service, StringComparison.Ordinal);
        Assert.Contains("GetMediaPresentationAsync", service, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", service, StringComparison.Ordinal);
    }
}
