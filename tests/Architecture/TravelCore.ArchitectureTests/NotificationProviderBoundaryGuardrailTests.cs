using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P25-T006 / P25-R3: provider-neutral delivery contracts without named production adapters or delivery persistence.
/// </summary>
public sealed class NotificationProviderBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void NotificationContracts_Expose_Provider_Port_And_Trust_Boundary()
    {
        Assert.NotNull(typeof(INotificationDeliveryProvider));
        Assert.NotNull(typeof(INotificationProviderResolver));
        Assert.NotNull(typeof(NotificationProviderKey));
        Assert.NotNull(typeof(NotificationProviderTrustBoundary));
        Assert.NotNull(typeof(NotificationProviderBoundary));
        Assert.True(NotificationOwnershipBoundary.ProviderPortImplemented);
        Assert.True(NotificationOwnershipBoundary.ProviderAbstractionImplemented);
        Assert.False(NotificationOwnershipBoundary.ProviderImplemented);
    }

    [Fact]
    public void NotificationProviderTrustBoundary_Keeps_Zero_Provider_Posture()
    {
        Assert.Equal("NONE", NotificationProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("PublisherCall != DeliverySuccess", NotificationProviderTrustBoundary.PublisherCallIsNotDeliverySuccess);
        Assert.Equal("ProviderAck != DownstreamCommit", NotificationProviderTrustBoundary.ProviderAckIsNotDownstreamCommit);
        Assert.True(NotificationProviderTrustBoundary.ProviderPortImplemented);
        Assert.True(NotificationProviderTrustBoundary.ZeroProviderPostureValid);
        Assert.False(NotificationProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(NotificationProviderTrustBoundary.ProductionProviderRegistered);
    }

    [Fact]
    public void NotificationProviderBoundary_Keeps_Adapters_And_Persistence_Out()
    {
        Assert.Equal("Notification", NotificationProviderBoundary.DeliveryOwner);
        Assert.True(NotificationProviderBoundary.NotificationOwnsProviderAbstraction);
        Assert.True(NotificationProviderBoundary.ProviderPortImplemented);
        Assert.False(NotificationProviderBoundary.NamedProductionAdapterImplemented);
        Assert.False(NotificationProviderBoundary.ProviderExecutionPersistenceImplemented);
        Assert.False(NotificationProviderBoundary.DeliveryStatePersistenceImplemented);
        Assert.False(NotificationProviderBoundary.SmtpClientImplemented);
        Assert.False(NotificationProviderBoundary.TwilioImplemented);
    }

    [Fact]
    public void NotificationProviderCapability_Lists_Planned_Delivery_Capabilities_Only()
    {
        var names = Enum.GetNames<NotificationProviderCapability>()
            .Where(name => name is not "None")
            .ToArray();
        Assert.Equal(3, names.Length);
        Assert.Contains("EmailDelivery", names);
        Assert.Contains("SmsDelivery", names);
        Assert.Contains("InAppDelivery", names);
    }

    [Fact]
    public void Notification_T006_Forbids_Named_Adapters_And_Delivery_Persistence_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(EmailProvider|SmsProvider|PushProvider|SmtpClient|TwilioClient|SendGridClient|NotificationDelivery|DeliveryAttempt|ChannelPreference|NotificationTemplate)\b",
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
            "Notification T006 forbids named adapters/persistence product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Notification_T006_Has_No_New_Migration_Or_Product_Table_Additions()
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
        Assert.False(NotificationProviderBoundary.ProviderExecutionPersistenceImplemented);
    }

    [Fact]
    public void P25_Evidence_Records_T006_And_R3()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P25-implementation-plan.md"));
        Assert.Contains("TC-P25-T006", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R3", plan, StringComparison.Ordinal);
        Assert.Contains("NotificationProviderBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
