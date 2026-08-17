using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T010 / TC-P11-T009 / TC-P12-T008: Public Tour detail — P09-R6 SEO default, app-proxy media,
/// published execution summaries (P11-R8) and optional public price facts (P12-R8).
/// Booking/Payment/Checkout/Availability remain forbidden.
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
                        @"\b(storageKey|DeletedAt|ArchivedAt|IsDeleted|SetIndexPolicy|admin/catalog/tours|BookingEngine|PriceQuote|BookableNow|BookingCta|Payment|Reservation|availabilityCount)\b",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            featureHits.Count == 0,
            "Public Tour detail must not leak StorageKey / Admin lifecycle / IndexPolicy / Booking commerce:\n"
            + string.Join('\n', featureHits));

        var loaderPath = Path.Combine(featureRoot, "load-tour-detail.ts");
        Assert.True(File.Exists(loaderPath), loaderPath);
        var loader = File.ReadAllText(loaderPath);
        Assert.Contains("media/presentation", loader, StringComparison.Ordinal);
        Assert.Contains("resolveMediaAppProxySrc", loader, StringComparison.Ordinal);
        Assert.Contains("mediaOriginalContentPath", loader, StringComparison.Ordinal);
        Assert.Contains("departures/published", loader, StringComparison.Ordinal);
        Assert.Contains("publishedDepartures", loader, StringComparison.Ordinal);
        Assert.Contains("/api/pricing/public/tour-departures", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/pricing/prices", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", loader, StringComparison.Ordinal);

        var viewPath = Path.Combine(featureRoot, "tour-detail-view.tsx");
        Assert.True(File.Exists(viewPath), viewPath);
        var view = File.ReadAllText(viewPath);
        Assert.DoesNotMatch(new Regex(@"\bHero\b"), view);
        Assert.Contains("publishedDepartures", view, StringComparison.Ordinal);
        Assert.Contains("priceSummary", view, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", view, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", view, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingCta", view, StringComparison.Ordinal);

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
        Assert.Contains("P11-R8", page, StringComparison.Ordinal);
        Assert.Contains("P12-R8", page, StringComparison.Ordinal);
        Assert.Contains("index: false, follow: true", page, StringComparison.Ordinal);
        Assert.Contains("robotsFromComposed", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", page, StringComparison.Ordinal);
        Assert.DoesNotContain("StorageKey", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking", page, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicPublishedDepartures_Endpoint_Filters_Published_Only()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Endpoints",
            "TourDepartureEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);
        var endpoints = File.ReadAllText(endpointsPath);
        Assert.Contains("departures/published", endpoints, StringComparison.Ordinal);
        Assert.Contains("ITourDeparturePublicQuery", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", endpoints, StringComparison.Ordinal);

        var queryPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Services",
            "TourDeparturePublicQuery.cs");
        Assert.True(File.Exists(queryPath), queryPath);
        var query = File.ReadAllText(queryPath);
        Assert.Contains("TourDepartureStatus.Published", query, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", query, StringComparison.Ordinal);
        Assert.DoesNotContain("IBookingService", query, StringComparison.Ordinal);
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
