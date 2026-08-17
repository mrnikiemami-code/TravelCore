using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P12-T006 / P12-R6: Access-backed Admin Pricing baseline — Pricing-owned, not Tour Admin.
/// </summary>
public sealed class PricingAdminAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PricingAdminEndpoints_Require_Prices_ReadWrite_Policies()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Infrastructure",
            "Endpoints",
            "PricingAdminEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.Pricing.Prices.Read", text, StringComparison.Ordinal);
        Assert.Contains("Access.Pricing.Prices.Write", text, StringComparison.Ordinal);
        Assert.Contains("/api/pricing/prices", text, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"MapDelete\s*\(", RegexOptions.CultureInvariant),
            text);
        Assert.DoesNotContain("/api/tour", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/quotes", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("checkout", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PaymentIntent", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ExchangeRate", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ITourDepartureAdminService", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AccessCatalog_Includes_PricingPrices_ReadWrite()
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
        Assert.Contains("pricing.prices.read", text, StringComparison.Ordinal);
        Assert.Contains("pricing.prices.write", text, StringComparison.Ordinal);

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
        Assert.Contains("Access.Pricing.Prices.Read", policies, StringComparison.Ordinal);
        Assert.Contains("Access.Pricing.Prices.Write", policies, StringComparison.Ordinal);

        var accessModulePath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Access",
            "TravelCore.Modules.Access.Infrastructure",
            "AccessModule.cs");
        var accessModule = File.ReadAllText(accessModulePath);
        Assert.Contains("pricing.prices.read", accessModule, StringComparison.Ordinal);
        Assert.Contains("pricing.prices.write", accessModule, StringComparison.Ordinal);
    }

    [Fact]
    public void TourAdmin_DoesNot_Own_PricingAdmin_Surface()
    {
        var tourEndpoints = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Endpoints");
        Assert.True(Directory.Exists(tourEndpoints), tourEndpoints);

        var hits = Directory.EnumerateFiles(tourEndpoints, "*.cs", SearchOption.TopDirectoryOnly)
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
                        @"/api/pricing|IPriceAdminService|PriceOccupancyRule|pricing\.prices",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour Admin must not own Pricing Admin API:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AdminPricingFrontend_IfPresent_Is_Not_Under_Tour_Admin()
    {
        var tourAdmin = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-tour");
        var departureAdmin = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "admin-departure");
        Assert.True(Directory.Exists(tourAdmin), tourAdmin);
        Assert.True(Directory.Exists(departureAdmin), departureAdmin);

        var hits = Directory.EnumerateFiles(tourAdmin, "*.*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(departureAdmin, "*.*", SearchOption.AllDirectories))
            .Where(p => p.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                        || p.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                            && Regex.IsMatch(
                                x.line,
                                @"/api/pricing|pricing\.prices|PriceOccupancyRule|QuoteWorkflow",
                                RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour/Departure Admin UI must not own Pricing Admin:\n" + string.Join('\n', hits));
    }
}
