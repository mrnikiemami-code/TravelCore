using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P11-T008: Access-backed Admin TourDeparture baseline — no Booking/Pricing invent.
/// </summary>
public sealed class TourDepartureAdminAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void TourDepartureEndpoints_Mutations_Require_DeparturesWrite_Policy()
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

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.Tour.Departures.Read", text, StringComparison.Ordinal);
        Assert.Contains("Access.Tour.Departures.Write", text, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"MapDelete\s*\(", RegexOptions.CultureInvariant),
            text);
        Assert.DoesNotContain("Booking", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Pricing", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment", text, StringComparison.Ordinal);
        Assert.DoesNotContain("BookableNow", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AccessCatalog_Includes_TourDepartures_ReadWrite()
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
        Assert.Contains("tour.departures.read", text, StringComparison.Ordinal);
        Assert.Contains("tour.departures.write", text, StringComparison.Ordinal);

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
        Assert.Contains("Access.Tour.Departures.Read", policies, StringComparison.Ordinal);
        Assert.Contains("Access.Tour.Departures.Write", policies, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminDepartureFrontend_Omits_BookingPricingPayment()
    {
        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-departure");
        Assert.True(Directory.Exists(featureRoot), featureRoot);

        var hits = Directory.EnumerateFiles(featureRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(featureRoot, "*.tsx", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"\b(Booking|Pricing|Payment|BookableNow|FlightSegment|HotelBooking|availabilityCalc)\b",
                                RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Admin Departure must not invent Booking/Pricing/Payment:\n" + string.Join('\n', hits));

        var pagePath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "admin",
            "catalog",
            "departures",
            "page.tsx");
        Assert.True(File.Exists(pagePath), pagePath);
        Assert.Contains("AdminShell", File.ReadAllText(pagePath), StringComparison.Ordinal);
    }
}
