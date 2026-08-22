using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.CommercialFinance.Contracts;
using TravelCore.Modules.CommercialFinance.Infrastructure;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P39-T006: independent Commercial Finance module and schema commercial_finance;
/// no cross-schema FK, no Payment event handlers, no commission formulas.
/// </summary>
public sealed class CommercialFinanceBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void CommercialFinanceProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.CommercialFinance.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.CommercialFinance.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.CommercialFinance.Infrastructure");
        Assert.Equal("commercial_finance", CommercialFinanceDbContext.SchemaName);
        Assert.Equal("commercial_finance", CommercialFinanceOwnershipBoundary.SchemaName);
        Assert.Equal("CommercialFinance", CommercialFinanceOwnershipBoundary.OwnerModule);
    }

    [Fact]
    public void CommercialFinanceInfrastructure_MustNotProjectReference_ForbiddenPeerModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.CommercialFinance.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "CommercialFinance.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void CommercialFinanceDomain_MustNotProjectReference_ForbiddenPeerModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.CommercialFinance.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "CommercialFinance.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void CommercialFinanceContracts_MustNotProjectReference_ForbiddenPeerModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.CommercialFinance.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "CommercialFinance.Contracts peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_Payment_AgencyMarketplace_DoNot_Depend_On_CommercialFinance()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Booking.Contracts",
                     "TravelCore.Modules.Booking.Domain",
                     "TravelCore.Modules.Booking.Infrastructure",
                     "TravelCore.Modules.Payment.Contracts",
                     "TravelCore.Modules.Payment.Domain",
                     "TravelCore.Modules.Payment.Infrastructure",
                     "TravelCore.Modules.AgencyMarketplace.Contracts",
                     "TravelCore.Modules.AgencyMarketplace.Domain",
                     "TravelCore.Modules.AgencyMarketplace.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.CommercialFinance", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference CommercialFinance:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void CommercialFinanceModule_Forbids_PeerSchemaFk_And_PaymentEventHandlers()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "CommercialFinance");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record)\s+(PaymentSucceededHandler|RefundEventHandler|CommissionCalculator|SettlementJob|PayoutProcessor|FxConverter|TaxCalculator)\b",
            RegexOptions.Compiled);

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

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Commercial Finance T006 forbids engines/handlers:\n" + string.Join('\n', hits));

        var fkHits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(identity|access|party|booking|payment|search|seo|content|destination|notification|trip_planner|b2b|agency_marketplace)""|AgencyMarketplaceDbContext|BookingDbContext|PaymentDbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            fkHits.Count == 0,
            "Commercial Finance must not introduce peer-schema FK:\n" + string.Join('\n', fkHits));
    }

    [Fact]
    public void Host_Registers_CommercialFinanceModule_After_Analytics()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        var analyticsIndex = program.IndexOf("new AnalyticsModule()", StringComparison.Ordinal);
        var financeIndex = program.IndexOf("new CommercialFinanceModule()", StringComparison.Ordinal);
        Assert.True(analyticsIndex >= 0, "AnalyticsModule must be registered.");
        Assert.True(financeIndex > analyticsIndex, "CommercialFinanceModule must register after AnalyticsModule.");
    }

    [Fact]
    public void AccessCatalog_Includes_CommercialFinance_Permissions()
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
        Assert.Contains(CommercialFinancePermissionCodes.AgreementsRead, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.AgreementsWrite, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.ObligationsRead, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.SettlementsRead, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.SettlementsApprove, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.PayoutsRead, text, StringComparison.Ordinal);
        Assert.Contains(CommercialFinancePermissionCodes.PayoutsApprove, text, StringComparison.Ordinal);
    }

    [Fact]
    public void CommercialFinanceAdminEndpoints_Require_Access_Policies()
    {
        var endpointsPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "CommercialFinance",
            "TravelCore.Modules.CommercialFinance.Infrastructure",
            "Endpoints",
            "CommercialFinanceAdminEndpoints.cs");
        var text = File.ReadAllText(endpointsPath);
        Assert.Contains("/api/commercial-finance/agreements", text, StringComparison.Ordinal);
        Assert.Contains("/api/commercial-finance/obligations", text, StringComparison.Ordinal);
        Assert.Contains("/api/commercial-finance/settlements", text, StringComparison.Ordinal);
        Assert.Contains("/periods", text, StringComparison.Ordinal);
        Assert.Contains("/api/commercial-finance/payouts", text, StringComparison.Ordinal);
        Assert.Contains("/instructions", text, StringComparison.Ordinal);
        Assert.Contains("Access.CommercialFinance.Agreements.Read", text, StringComparison.Ordinal);
        Assert.Contains("Access.CommercialFinance.Obligations.Read", text, StringComparison.Ordinal);
        Assert.DoesNotContain("commission_rate", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OwnershipBoundary_Locks_Inequalities()
    {
        Assert.False(CommercialFinanceOwnershipBoundary.MutatesAgencyOffer);
        Assert.False(CommercialFinanceOwnershipBoundary.CommissionFormulaImplemented);
        Assert.False(CommercialFinanceOwnershipBoundary.PaymentEventHandlerImplemented);
        Assert.Equal("Settlement != Payment", CommercialFinanceOwnershipBoundary.SettlementIsNotPayment);
        Assert.Equal("Payout != Booking", CommercialFinanceOwnershipBoundary.PayoutIsNotBooking);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForbiddenPeer(string name) =>
        name is "TravelCore.Modules.Identity.Infrastructure"
            or "TravelCore.Modules.Identity.Domain"
            or "TravelCore.Modules.Identity.Contracts"
            or "TravelCore.Modules.Access.Infrastructure"
            or "TravelCore.Modules.Access.Domain"
            or "TravelCore.Modules.Access.Contracts"
            or "TravelCore.Modules.Party.Infrastructure"
            or "TravelCore.Modules.Party.Domain"
            or "TravelCore.Modules.Party.Contracts"
            or "TravelCore.Modules.Booking.Infrastructure"
            or "TravelCore.Modules.Booking.Domain"
            or "TravelCore.Modules.Booking.Contracts"
            or "TravelCore.Modules.Payment.Infrastructure"
            or "TravelCore.Modules.Payment.Domain"
            or "TravelCore.Modules.Payment.Contracts"
            or "TravelCore.Modules.AgencyMarketplace.Infrastructure"
            or "TravelCore.Modules.AgencyMarketplace.Domain"
            or "TravelCore.Modules.AgencyMarketplace.Contracts"
            or "TravelCore.Modules.Search.Infrastructure"
            or "TravelCore.Modules.Search.Domain"
            or "TravelCore.Modules.Search.Contracts"
            or "TravelCore.Modules.Seo.Infrastructure"
            or "TravelCore.Modules.Seo.Domain"
            or "TravelCore.Modules.Seo.Contracts"
            or "TravelCore.Modules.Content.Infrastructure"
            or "TravelCore.Modules.Content.Domain"
            or "TravelCore.Modules.Content.Contracts"
            or "TravelCore.Modules.Destination.Infrastructure"
            or "TravelCore.Modules.Destination.Domain"
            or "TravelCore.Modules.Destination.Contracts"
            or "TravelCore.Modules.Notification.Infrastructure"
            or "TravelCore.Modules.Notification.Domain"
            or "TravelCore.Modules.Notification.Contracts"
            or "TravelCore.Modules.TripPlanner.Infrastructure"
            or "TravelCore.Modules.TripPlanner.Domain"
            or "TravelCore.Modules.TripPlanner.Contracts"
            or "TravelCore.Modules.B2B.Infrastructure"
            or "TravelCore.Modules.B2B.Domain"
            or "TravelCore.Modules.B2B.Contracts"
            or "TravelCore.Observability";
}
