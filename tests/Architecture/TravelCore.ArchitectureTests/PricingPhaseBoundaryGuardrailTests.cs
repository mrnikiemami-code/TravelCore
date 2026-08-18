using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P12-T009: Phase boundary evidence — Pricing independent of Tour; Price ≠ Quote ≠ Payment;
/// occupancy ≠ Booking passenger; public read-only (no Book Now); FX conversion out of Pricing.
/// </summary>
public sealed class PricingPhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P12_EvidencePack_Exists()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P12-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P12-R1", text, StringComparison.Ordinal);
        Assert.Contains("P12-R8", text, StringComparison.Ordinal);
        Assert.Contains("Price ≠ Quote ≠ Payment", text, StringComparison.Ordinal);
        Assert.Contains("no Book Now", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TC-P12-GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P12-GATE COMPLETE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Price_And_Quote_Remain_Distinct_With_Immutable_Snapshot()
    {
        var domain = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Domain");
        Assert.True(File.Exists(Path.Combine(domain, "Price.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "Quote.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "QuoteSnapshotComponent.cs")));

        var price = File.ReadAllText(Path.Combine(domain, "Price.cs"));
        Assert.DoesNotContain("class Quote", price, StringComparison.Ordinal);
        Assert.DoesNotContain("ExpiresAt", price, StringComparison.Ordinal);

        var quote = File.ReadAllText(Path.Combine(domain, "Quote.cs"));
        Assert.Contains("ExpiresAt", quote, StringComparison.Ordinal);
        Assert.Contains("SnapshotComponents", quote, StringComparison.Ordinal);
        Assert.Contains("IsExpired", quote, StringComparison.Ordinal);
        Assert.DoesNotContain("class Price ", quote, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\bpublic\s+void\s+(Add|Replace|Update|Remove)"), quote);
        Assert.DoesNotContain("PaymentId", quote, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingId", quote, StringComparison.Ordinal);
    }

    [Fact]
    public void Occupancy_Rules_Are_Pricing_Facts_Not_Booking_Passenger()
    {
        var domain = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Domain");
        Assert.True(File.Exists(Path.Combine(domain, "PriceOccupancyRule.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "PassengerCategory.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "OccupancyCategory.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "Passenger.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "BookingPassenger.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "ReservationPassenger.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "Traveler.cs")));

        var occupancy = File.ReadAllText(Path.Combine(domain, "PriceOccupancyRule.cs"));
        Assert.Contains("PassengerCategory", occupancy, StringComparison.Ordinal);
        Assert.Contains("OccupancyCategory", occupancy, StringComparison.Ordinal);
        Assert.DoesNotContain("Passport", occupancy, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DateOfBirth", occupancy, StringComparison.Ordinal);
        Assert.DoesNotContain("ReservationId", occupancy, StringComparison.Ordinal);

        var category = File.ReadAllText(Path.Combine(domain, "PassengerCategory.cs"));
        Assert.Contains("enum PassengerCategory", category, StringComparison.Ordinal);
        Assert.DoesNotContain("class Passenger", category, StringComparison.Ordinal);
    }

    [Fact]
    public void Public_Tour_Detail_Has_No_Book_Now_Or_Checkout()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "tour-detail");
        var viewPath = Path.Combine(featureRoot, "tour-detail-view.tsx");
        var view = File.ReadAllText(viewPath);
        Assert.Contains("priceSummary", view, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", view, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingCta", view, StringComparison.Ordinal);
        Assert.DoesNotContain("رزرو کنید", view, StringComparison.Ordinal);

        // Ignore JSDoc/comments that document the "no checkout" invariant itself.
        var viewNonComments = string.Join(
            '\n',
            File.ReadAllLines(viewPath)
                .Where(line =>
                {
                    var trimmed = line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("*", StringComparison.Ordinal)
                        && !trimmed.StartsWith("/*", StringComparison.Ordinal);
                }));
        Assert.DoesNotContain("checkout", viewNonComments, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("BookingCtaIsland", viewNonComments, StringComparison.Ordinal);

        var loader = File.ReadAllText(Path.Combine(featureRoot, "load-tour-detail.ts"));
        Assert.Contains("/api/pricing/public/tour-departures", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/pricing/prices", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateQuote", loader, StringComparison.Ordinal);

        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
    }
}
