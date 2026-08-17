using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P11-T001: TourDeparture scaffolding boundary — Departure ∈ Tour; no Booking/Pricing ownership.
/// </summary>
public sealed class TourDepartureBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void TourDeparture_Scaffolding_Lives_In_Tour_Module_Only()
    {
        var domain = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Domain");
        Assert.True(File.Exists(Path.Combine(domain, "TourDeparture.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "TourDepartureId.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "TourProduct.cs")));

        // No duplicate TourProduct model outside Tour.
        var backendModules = Path.Combine(RepoRoot, "src", "backend", "Modules");
        var rogueProducts = Directory.EnumerateFiles(backendModules, "TourProduct.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Tour{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.True(rogueProducts.Count == 0, "Duplicate TourProduct models:\n" + string.Join('\n', rogueProducts));
    }

    [Fact]
    public void TourModule_Forbids_Booking_Pricing_Engines_And_P11_Later_Types()
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
                        @"\b(class|record|enum|struct|interface)\s+(FlightSegment|TourHotelOption|BookingEngine|PriceQuote|SearchIndex|HotelBooking|TourBooking|TourPrice|TourPricing|TourInventory)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(IBookingService|IPricingService|ITourSearchService)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T001 scaffolding must not introduce Booking/Pricing/Flight/HotelOption product:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void TourInfrastructure_MustNot_ProjectReference_Booking_Or_Pricing()
    {
        var tourInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Tour.Infrastructure");
        var violations = tourInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains("Booking", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Pricing", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Payment", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Tour.Infrastructure must not depend on Booking/Pricing/Payment:\n"
            + string.Join('\n', violations));
    }
}
