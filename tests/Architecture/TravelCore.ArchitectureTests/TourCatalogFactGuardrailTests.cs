using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P09-T006: Tour services/policies/requirements stay Tour-owned descriptive facts.
/// </summary>
public sealed class TourCatalogFactGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourCatalogFacts_DoNotLeakBookingPricingPaymentEngines()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"\b(class|record|struct|enum)\s+(BookingCancellation|PaymentRule|PricingEngine|FlightSegment|HotelBooking)\b")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour catalog facts must not introduce Booking/Payment/Pricing engines:\n"
            + string.Join('\n', hits));

        Assert.True(File.Exists(Path.Combine(
            tourRoot, "TravelCore.Modules.Tour.Domain", "TourCatalogFacts.cs")));
    }
}
