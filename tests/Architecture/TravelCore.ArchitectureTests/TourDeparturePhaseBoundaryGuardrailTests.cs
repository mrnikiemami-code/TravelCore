using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P11-T010: Phase boundary evidence — TourProduct ≠ TourDeparture; no Booking/Pricing/Payment/Flight/Hotel ownership.
/// </summary>
public sealed class TourDeparturePhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourProduct_And_TourDeparture_Remain_Distinct_Types()
    {
        var domain = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Domain");
        Assert.True(File.Exists(Path.Combine(domain, "TourProduct.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "TourDeparture.cs")));

        var product = File.ReadAllText(Path.Combine(domain, "TourProduct.cs"));
        Assert.DoesNotContain("class TourDeparture", product, StringComparison.Ordinal);
        Assert.DoesNotContain(": TourDeparture", product, StringComparison.Ordinal);

        var departure = File.ReadAllText(Path.Combine(domain, "TourDeparture.cs"));
        Assert.Contains("TourProductId", departure, StringComparison.Ordinal);
        Assert.DoesNotContain("class TourProduct", departure, StringComparison.Ordinal);
    }

    [Fact]
    public void TourModule_Forbids_Booking_Pricing_Payment_Flight_Hotel_Ownership_Types()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
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
                        @"\b(class|record|enum|struct|interface)\s+(FlightSegment|FlightService|Airline|TourHotelOption|HotelBooking|BookingEngine|PriceQuote|PaymentIntent|SearchIndex|TourInventory)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(IBookingService|IPricingService|IPaymentService|ITourSearchService|IFlightService)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "P11 must not introduce Booking/Pricing/Payment/Flight/Hotel ownership types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PublicPublished_Does_Not_Expose_Bookable_Signals()
    {
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
        Assert.DoesNotContain("BookableNow", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvailableSeats", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PriceQuote", query, StringComparison.OrdinalIgnoreCase);

        var loaderPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "tour-detail",
            "load-tour-detail.ts");
        var loader = File.ReadAllText(loaderPath);
        Assert.Contains("departures/published", loader, StringComparison.Ordinal);
        Assert.Contains("/api/pricing/public/tour-departures", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingCta", loader, StringComparison.OrdinalIgnoreCase);
    }
}
