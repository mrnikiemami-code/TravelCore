using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P13-T006 / P13-R6: Agency Panel is Agency Marketplace-owned, not Tour Admin / Identity commerce.
/// </summary>
public sealed class AgencyMarketplacePanelAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void AgencyMarketplacePanelEndpoints_Require_Access_Policies()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "AgencyMarketplace",
            "TravelCore.Modules.AgencyMarketplace.Infrastructure",
            "Endpoints",
            "AgencyMarketplacePanelEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("Access.AgencyMarketplace.Profile.Read", text, StringComparison.Ordinal);
        Assert.Contains("Access.AgencyMarketplace.Profile.Write", text, StringComparison.Ordinal);
        Assert.Contains("Access.AgencyMarketplace.Offers.Read", text, StringComparison.Ordinal);
        Assert.Contains("Access.AgencyMarketplace.Offers.Write", text, StringComparison.Ordinal);
        Assert.Contains("Access.AgencyMarketplace.Offers.Moderate", text, StringComparison.Ordinal);
        Assert.Contains("/submit", text, StringComparison.Ordinal);
        Assert.Contains("/approve", text, StringComparison.Ordinal);
        Assert.Contains("/publish", text, StringComparison.Ordinal);
        Assert.Contains("/suspend", text, StringComparison.Ordinal);
        Assert.Contains("/retire", text, StringComparison.Ordinal);
        Assert.Contains("EnsureOfferOwnedByAgencyAsync", text, StringComparison.Ordinal);
        Assert.Contains("IAccountAssociationQuery", text, StringComparison.Ordinal);
        Assert.Contains("/api/agency-marketplace/profiles", text, StringComparison.Ordinal);
        Assert.Contains("/api/agency-marketplace/offers", text, StringComparison.Ordinal);
        Assert.DoesNotContain("commission", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("settlement", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Booking", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/tour", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AgencyMarketplaceAdminEndpoints_Require_Moderate_And_Self_Guard()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "AgencyMarketplace",
            "TravelCore.Modules.AgencyMarketplace.Infrastructure",
            "Endpoints",
            "AgencyMarketplaceAdminEndpoints.cs");
        Assert.True(File.Exists(endpointsPath), endpointsPath);

        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("/api/agency-marketplace/moderation/offers", text, StringComparison.Ordinal);
        Assert.Contains("/pending", text, StringComparison.Ordinal);
        Assert.Contains("/approve", text, StringComparison.Ordinal);
        Assert.Contains("/reject", text, StringComparison.Ordinal);
        Assert.Contains("/suspend", text, StringComparison.Ordinal);
        Assert.Contains("/policy-evaluation", text, StringComparison.Ordinal);
        Assert.Contains("Access.AgencyMarketplace.Offers.Moderate", text, StringComparison.Ordinal);
        Assert.Contains("IAgencyOfferGovernanceService", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/tour", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AccessCatalog_Includes_AgencyMarketplace_Panel_Permissions()
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
        Assert.Contains("agency.marketplace.profile.read", text, StringComparison.Ordinal);
        Assert.Contains("agency.marketplace.profile.write", text, StringComparison.Ordinal);
        Assert.Contains("agency.marketplace.offers.read", text, StringComparison.Ordinal);
        Assert.Contains("agency.marketplace.offers.write", text, StringComparison.Ordinal);
        Assert.Contains("agency.marketplace.offers.moderate", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TourAdmin_DoesNot_Own_AgencyMarketplacePanel_Surface()
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
                        @"/api/agency-marketplace|IAgencyMarketplacePanelService|agency\.marketplace",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Tour Admin must not own Agency Marketplace panel API:\n" + string.Join('\n', hits));
    }
}
