using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P25-T004 / P25-R1: independent Notification module and schema notification;
/// Booking/Payment/TripPlanner/B2B execution ownership unchanged.
/// </summary>
public sealed class NotificationBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void NotificationProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Notification.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Notification.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Notification.Infrastructure");
        Assert.Equal("notification", NotificationDbContext.SchemaName);
        Assert.Equal("notification", NotificationOwnershipBoundary.SchemaName);
        Assert.Equal("Notification", NotificationOwnershipBoundary.OwnerModule);
    }

    [Fact]
    public void NotificationInfrastructure_MustNotProjectReference_ForbiddenPeerModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Notification.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Notification.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void NotificationDomain_MustNotProjectReference_ForbiddenPeerModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Notification.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Notification.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void NotificationContracts_MustNotProjectReference_ForbiddenPeerModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Notification.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Notification.Contracts peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_Payment_And_TripPlanner_DoNot_Depend_On_Notification()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Booking.Contracts",
                     "TravelCore.Modules.Booking.Domain",
                     "TravelCore.Modules.Booking.Infrastructure",
                     "TravelCore.Modules.Payment.Contracts",
                     "TravelCore.Modules.Payment.Domain",
                     "TravelCore.Modules.Payment.Infrastructure",
                     "TravelCore.Modules.TripPlanner.Contracts",
                     "TravelCore.Modules.TripPlanner.Domain",
                     "TravelCore.Modules.TripPlanner.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.Notification", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference Notification:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void NotificationModule_DoesNotOwn_Forbidden_T004_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(EmailProvider|SmsProvider|PushProvider|NotificationTemplate|DeliveryAttempt|NotificationDelivery|ChannelPreference|WebhookEndpoint|INotificationProvider|ISmtpClient)\b",
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

        Assert.True(hits.Count == 0, "Notification T004 forbids early product entities:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void NotificationModule_Forbids_PeerSchemaFk_And_SharedDbContext()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(identity|access|party|booking|payment|trip_planner|b2b)""|HasOne<.*(Identity|Access|Party|Booking|Payment|TripPlanner|B2B)|TravelCore\.Modules\.(Identity|Access|Party|Booking|Payment|TripPlanner|B2B)\.(Domain|Infrastructure)|(Identity|Access|Party|Booking|Payment|TripPlanner|B2B)DbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Notification must not introduce peer-schema FK/nav or share foreign DbContexts:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Host_Registers_NotificationModule_After_B2B_With_No_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        var b2bIndex = program.IndexOf("new B2BModule()", StringComparison.Ordinal);
        var notificationIndex = program.IndexOf("new NotificationModule()", StringComparison.Ordinal);
        Assert.True(b2bIndex >= 0, "B2BModule must be registered.");
        Assert.True(notificationIndex > b2bIndex, "NotificationModule must register after B2BModule.");

        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Notification",
            "TravelCore.Modules.Notification.Infrastructure",
            "NotificationModule.cs"));
        Assert.Contains("AddDbContext<NotificationDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/notification", module, StringComparison.Ordinal);
    }

    [Fact]
    public void Notification_Does_Not_Add_Payment_Target_Or_Change_Execution_Ownership()
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
        Assert.DoesNotContain("Notification", payment, StringComparison.Ordinal);

        Assert.Equal("Booking", NotificationPublisherBoundary.BookingPublisherOwner);
        Assert.Equal("Payment", NotificationPublisherBoundary.PaymentPublisherOwner);
        Assert.Equal("Notification", NotificationPublisherBoundary.DeliveryOwner);
        Assert.False(NotificationOwnershipBoundary.OwnsBookingExecution);
        Assert.False(NotificationOwnershipBoundary.OwnsPaymentExecution);
        Assert.False(NotificationOwnershipBoundary.ModifiesPaymentTargets);

        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
    }

    [Fact]
    public void Notification_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P25-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P25-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `notification`", text, StringComparison.Ordinal);
        Assert.Contains("Notification != Booking", text, StringComparison.Ordinal);
        Assert.Contains("Notification != Payment", text, StringComparison.Ordinal);
        Assert.Contains("TC-P25-T004", text, StringComparison.Ordinal);
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
            or "TravelCore.Modules.TripPlanner.Infrastructure"
            or "TravelCore.Modules.TripPlanner.Domain"
            or "TravelCore.Modules.TripPlanner.Contracts"
            or "TravelCore.Modules.B2B.Infrastructure"
            or "TravelCore.Modules.B2B.Domain"
            or "TravelCore.Modules.B2B.Contracts";
}
