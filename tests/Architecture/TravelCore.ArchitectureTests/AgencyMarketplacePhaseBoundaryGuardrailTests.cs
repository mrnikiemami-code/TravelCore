using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P13-T009: Phase boundary evidence — Agency ≠ Party ≠ Pricing ≠ Booking;
/// CatalogStatus ≠ PublicationStatus ≠ IndexPolicy; T008 vacant (publishing was T007).
/// </summary>
public sealed class AgencyMarketplacePhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P13_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P13-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P13-R1", text, StringComparison.Ordinal);
        Assert.Contains("P13-R7", text, StringComparison.Ordinal);
        Assert.Contains("Published Offer ≠ SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("TC-P13-T008", text, StringComparison.Ordinal);
        Assert.Contains("vacant", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TC-P13-GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P13-GATE COMPLETE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AgencyOffer_Publication_Is_Not_Catalog_Or_Seo()
    {
        var offerPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "AgencyMarketplace",
            "TravelCore.Modules.AgencyMarketplace.Domain",
            "AgencyOffer.cs");
        var statusPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "AgencyMarketplace",
            "TravelCore.Modules.AgencyMarketplace.Domain",
            "AgencyOfferPublicationStatus.cs");
        Assert.True(File.Exists(offerPath), offerPath);
        Assert.True(File.Exists(statusPath), statusPath);

        var offer = File.ReadAllText(offerPath);
        Assert.Contains("PublicationStatus", offer, StringComparison.Ordinal);
        Assert.Contains("Submit(", offer, StringComparison.Ordinal);
        Assert.Contains("Approve(", offer, StringComparison.Ordinal);
        Assert.Contains("Publish(", offer, StringComparison.Ordinal);
        Assert.DoesNotContain("IndexPolicy", StripComments(offer), StringComparison.Ordinal);
        Assert.DoesNotContain("CatalogStatus", StripComments(offer), StringComparison.Ordinal);

        var status = File.ReadAllText(statusPath);
        Assert.Contains("Draft", status, StringComparison.Ordinal);
        Assert.Contains("Submitted", status, StringComparison.Ordinal);
        Assert.Contains("Approved", status, StringComparison.Ordinal);
        Assert.Contains("Published", status, StringComparison.Ordinal);
        Assert.Contains("Rejected", status, StringComparison.Ordinal);
        Assert.Contains("Archived", status, StringComparison.Ordinal);
    }

    [Fact]
    public void AgencyMarketplace_DoesNot_Own_Price_Capacity_Or_Booking()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "AgencyMarketplace");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
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
                        @"\b(PriceOverride|CommissionRate|AvailableSeats|ReservedSeats|IndexPolicy|CatalogStatus|BookingId|PaymentId)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Agency Marketplace must not own Price/capacity/SEO/Booking identifiers:\n"
            + string.Join('\n', hits));

        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
    }

    [Fact]
    public void AgencyPanel_Has_No_Booking_Or_Seo_Ownership()
    {
        var page = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "agency", "page.tsx");
        Assert.True(File.Exists(page), page);
        var text = File.ReadAllText(page);
        Assert.Contains("Agency Marketplace", text, StringComparison.Ordinal);
        Assert.Contains("Published Offer ≠ SEO Indexed", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingCta", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/seo", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/tour", text, StringComparison.Ordinal);
        Assert.Contains("No Booking/Payment/Commission", text, StringComparison.Ordinal);
    }

    private static string StripComments(string source) =>
        string.Join(
            '\n',
            source.Split('\n')
                .Where(line =>
                {
                    var trimmed = line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("///", StringComparison.Ordinal);
                }));

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
