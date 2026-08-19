using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P25-T007 / P25-R4 + P25-R6: template orchestration and event consumption boundaries without persistence or runtime consumers.
/// </summary>
public sealed class NotificationEventTemplateBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void NotificationContracts_Expose_Event_And_Template_Ports()
    {
        Assert.NotNull(typeof(INotificationSemanticEventConsumer));
        Assert.NotNull(typeof(INotificationTemplateOrchestrator));
        Assert.NotNull(typeof(NotificationSemanticEventEnvelope));
        Assert.NotNull(typeof(NotificationIdempotencyBoundary));
        Assert.NotNull(typeof(NotificationTemplateBoundary));
        Assert.NotNull(typeof(NotificationEventConsumptionBoundary));
        Assert.True(NotificationOwnershipBoundary.TemplateOrchestrationImplemented);
        Assert.True(NotificationOwnershipBoundary.EventConsumptionBoundaryImplemented);
    }

    [Fact]
    public void NotificationIdempotencyBoundary_Keeps_Downstream_Posture()
    {
        Assert.Equal("FailedDelivery != SourceOfRecordRollback", NotificationIdempotencyBoundary.FailedDeliveryDoesNotRollbackSourceOfRecord);
        Assert.Equal("NOT ASSUMED", NotificationIdempotencyBoundary.ExactlyOnceDelivery);
        Assert.True(NotificationIdempotencyBoundary.DownstreamAsyncConsumerPortImplemented);
        Assert.True(NotificationIdempotencyBoundary.IdempotentDeliveryPostureDeclared);
        Assert.False(NotificationIdempotencyBoundary.DeliveryStatePersistenceImplemented);
        Assert.False(NotificationIdempotencyBoundary.OutboxConsumerImplemented);
    }

    [Fact]
    public void NotificationTemplateBoundary_Keeps_Rendering_And_Persistence_Out()
    {
        Assert.Equal("Notification", NotificationTemplateBoundary.TemplateOrchestrationOwner);
        Assert.True(NotificationTemplateBoundary.NotificationOwnsTemplateOrchestration);
        Assert.False(NotificationTemplateBoundary.TemplatePersistenceImplementedFlag);
        Assert.False(NotificationTemplateBoundary.TemplateRenderingImplemented);
    }

    [Fact]
    public void NotificationEventConsumptionBoundary_Keeps_Async_Consumer_Only()
    {
        Assert.Equal("Notification", NotificationEventConsumptionBoundary.DownstreamConsumerOwner);
        Assert.True(NotificationEventConsumptionBoundary.EventConsumptionBoundaryImplemented);
        Assert.True(NotificationEventConsumptionBoundary.SemanticEventConsumerPortImplemented);
        Assert.False(NotificationEventConsumptionBoundary.OutboxConsumerImplemented);
        Assert.False(NotificationEventConsumptionBoundary.SynchronousPublisherToProviderCallImplemented);
    }

    [Fact]
    public void NotificationSemanticEventKind_Lists_Planned_Kinds_Only()
    {
        var names = Enum.GetNames<NotificationSemanticEventKind>()
            .Where(name => name is not "Unknown")
            .ToArray();
        Assert.Equal(3, names.Length);
        Assert.Contains("LeadSubmitted", names);
        Assert.Contains("BookingConfirmed", names);
        Assert.Contains("PaymentSucceeded", names);
    }

    [Fact]
    public void Notification_T007_Forbids_Persistence_And_Runtime_Consumer_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(NotificationDelivery|DeliveryAttempt|NotificationTemplateEntity|TemplateStore|OutboxConsumer|NotificationOutbox|RenderedTemplate|TemplateVersion)\b",
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
            "Notification T007 forbids persistence/runtime consumer product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Notification_T007_Has_No_New_Migration_Or_Product_Table_Additions()
    {
        var migrationsDir = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Notification",
            "TravelCore.Modules.Notification.Infrastructure",
            "Migrations");
        var migrationFiles = Directory.Exists(migrationsDir)
            ? Directory.GetFiles(migrationsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)
                    && !f.Contains("InitialNotificationScaffolding", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];
        Assert.Empty(migrationFiles);
        Assert.False(NotificationEventConsumptionBoundary.DeliveryOrchestrationPersistenceImplemented);
    }

    [Fact]
    public void P25_Evidence_Records_T007_And_R4_R6()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P25-implementation-plan.md"));
        Assert.Contains("TC-P25-T007", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R4", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R6", plan, StringComparison.Ordinal);
        Assert.Contains("NotificationTemplateBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("NotificationEventConsumptionBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
