using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P10-T009: consolidated Experience boundary / dual-SoR hardening.
/// </summary>
public sealed class ExperiencePhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P10_EvidencePack_Exists()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P10-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P10-R1", text, StringComparison.Ordinal);
        Assert.Contains("P10-R8", text, StringComparison.Ordinal);
        Assert.Contains("Published ≠ bookable", text, StringComparison.Ordinal);
        Assert.Contains("DEFERRED", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TourModule_ForbidsBookingPricingDepartureInventorySearchTypes()
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
                        @"\b(class|record|enum|struct|interface)\s+(TourDeparture|FlightSegment|TourHotelOption|HotelBooking|TourBooking|TourPrice|TourPricing|TourInventory|TourSearchIndex)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "P10 Experience must not introduce Departure/Booking/Pricing/Inventory/Search types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Experience_DoesNotDuplicateCoverOrCatalogStatusSources()
    {
        var domain = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour", "TravelCore.Modules.Tour.Domain");
        Assert.False(File.Exists(Path.Combine(domain, "ExperienceMediaLink.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "ExperienceCatalogStatus.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "ExperiencePublicationState.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "TourProductMediaLink.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "TourCatalogStatus.cs")));
        Assert.True(File.Exists(Path.Combine(domain, "ExperiencePublishability.cs")));
    }
}
