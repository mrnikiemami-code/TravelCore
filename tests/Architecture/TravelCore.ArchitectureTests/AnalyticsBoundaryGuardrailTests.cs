using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P27-T004 / P27-R1: independent Analytics module and schema analytics;
/// Search/Booking/Payment/Notification/Observability execution ownership unchanged.
/// </summary>
public sealed class AnalyticsBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void AnalyticsProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Analytics.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Analytics.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Analytics.Infrastructure");
        Assert.Equal("analytics", AnalyticsDbContext.SchemaName);
        Assert.Equal("analytics", AnalyticsOwnershipBoundary.SchemaName);
        Assert.Equal("Analytics", AnalyticsOwnershipBoundary.OwnerModule);
    }

    [Fact]
    public void AnalyticsInfrastructure_MustNotProjectReference_ForbiddenPeerModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Analytics.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Analytics.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AnalyticsDomain_MustNotProjectReference_ForbiddenPeerModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Analytics.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Analytics.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AnalyticsContracts_MustNotProjectReference_ForbiddenPeerModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Analytics.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Analytics.Contracts peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Search_Booking_Payment_And_Notification_DoNot_Depend_On_Analytics()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Search.Contracts",
                     "TravelCore.Modules.Search.Domain",
                     "TravelCore.Modules.Search.Infrastructure",
                     "TravelCore.Modules.Booking.Contracts",
                     "TravelCore.Modules.Booking.Domain",
                     "TravelCore.Modules.Booking.Infrastructure",
                     "TravelCore.Modules.Payment.Contracts",
                     "TravelCore.Modules.Payment.Domain",
                     "TravelCore.Modules.Payment.Infrastructure",
                     "TravelCore.Modules.Notification.Contracts",
                     "TravelCore.Modules.Notification.Domain",
                     "TravelCore.Modules.Notification.Infrastructure",
                     "TravelCore.Modules.Seo.Contracts",
                     "TravelCore.Modules.Seo.Domain",
                     "TravelCore.Modules.Seo.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.Analytics", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference Analytics:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void AnalyticsModule_DoesNotOwn_Forbidden_T004_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(MixpanelProvider|GoogleAnalyticsProvider|AmplitudeProvider|SegmentProvider|AnalyticsEventStore|EventWarehouse|AnalyticsDashboard|IAnalyticsVendorClient|AnalyticsWarehouse|StreamingAnalyticsBus)\b",
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

        Assert.True(hits.Count == 0, "Analytics T004 forbids early product entities:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void AnalyticsModule_Forbids_PeerSchemaFk_And_SharedDbContext()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(identity|access|party|booking|payment|search|seo|content|destination|notification|trip_planner|b2b)""|HasOne<.*(Identity|Access|Party|Booking|Payment|Search|Seo|Content|Destination|Notification|TripPlanner|B2B)|TravelCore\.Modules\.(Identity|Access|Party|Booking|Payment|Search|Seo|Content|Destination|Notification|TripPlanner|B2B)\.(Domain|Infrastructure)|(Identity|Access|Party|Booking|Payment|Search|Seo|Content|Destination|Notification|TripPlanner|B2B)DbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Analytics must not introduce peer-schema FK/nav or share foreign DbContexts:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Host_Registers_AnalyticsModule_After_Notification_With_No_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        var notificationIndex = program.IndexOf("new NotificationModule()", StringComparison.Ordinal);
        var analyticsIndex = program.IndexOf("new AnalyticsModule()", StringComparison.Ordinal);
        Assert.True(notificationIndex >= 0, "NotificationModule must be registered.");
        Assert.True(analyticsIndex > notificationIndex, "AnalyticsModule must register after NotificationModule.");

        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Analytics",
            "TravelCore.Modules.Analytics.Infrastructure",
            "AnalyticsModule.cs"));
        Assert.Contains("AddDbContext<AnalyticsDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/analytics", module, StringComparison.Ordinal);
    }

    [Fact]
    public void Analytics_Does_Not_Add_Payment_Target_Or_Change_Execution_Ownership()
    {
        var payment = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "PaymentTargetKind.cs"));
        Assert.Contains("TourBooking", payment, StringComparison.Ordinal);
        Assert.Contains("HotelBooking", payment, StringComparison.Ordinal);
        Assert.Contains("FlightBooking", payment, StringComparison.Ordinal);
        Assert.DoesNotContain("Analytics", payment, StringComparison.Ordinal);

        Assert.Equal("Booking", AnalyticsPublisherBoundary.BookingPublisherOwner);
        Assert.Equal("Payment", AnalyticsPublisherBoundary.PaymentPublisherOwner);
        Assert.Equal("Analytics", AnalyticsPublisherBoundary.DispatchOwner);
        Assert.False(AnalyticsOwnershipBoundary.OwnsBookingExecution);
        Assert.False(AnalyticsOwnershipBoundary.OwnsPaymentExecution);
        Assert.False(AnalyticsOwnershipBoundary.OwnsSearchRanking);
        Assert.False(AnalyticsOwnershipBoundary.OwnsPlatformTelemetry);
        Assert.False(AnalyticsOwnershipBoundary.ModifiesPaymentTargets);

        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
    }

    [Fact]
    public void Analytics_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P27-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P27-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `analytics`", text, StringComparison.Ordinal);
        Assert.Contains("Analytics != Booking", text, StringComparison.Ordinal);
        Assert.Contains("Analytics != Booking/Payment/Search", text, StringComparison.Ordinal);
        Assert.Contains("Notification/Observability", text, StringComparison.Ordinal);
        Assert.Contains("TC-P27-T004", text, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
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
