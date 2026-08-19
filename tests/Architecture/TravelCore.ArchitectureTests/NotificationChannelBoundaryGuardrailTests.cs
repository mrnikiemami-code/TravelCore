using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P25-T005 / P25-R2: channel taxonomy boundary without provider execution or channel persistence.
/// </summary>
public sealed class NotificationChannelBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void NotificationDomain_Exposes_ChannelBoundary_Models()
    {
        Assert.NotNull(typeof(NotificationChannelKind));
        Assert.NotNull(typeof(NotificationChannelBoundary));
        Assert.NotNull(typeof(NotificationChannelReference));
        Assert.True(NotificationOwnershipBoundary.ChannelBoundaryImplemented);
        Assert.False(NotificationOwnershipBoundary.ChannelPersistenceImplemented);
        Assert.False(NotificationOwnershipBoundary.ProviderImplemented);
    }

    [Fact]
    public void NotificationChannelBoundary_Keeps_Provider_And_Persistence_Out()
    {
        Assert.Equal("Email · SMS · In-app", NotificationChannelBoundary.ChannelTaxonomy);
        Assert.Equal("Notification", NotificationChannelBoundary.DeliveryOwner);
        Assert.True(NotificationChannelBoundary.NotificationOwnsChannelTaxonomy);
        Assert.False(NotificationChannelBoundary.ChannelPersistenceImplemented);
        Assert.False(NotificationChannelBoundary.ProviderExecutionImplemented);
        Assert.False(NotificationChannelBoundary.SmtpClientImplemented);
        Assert.False(NotificationChannelBoundary.TwilioImplemented);
    }

    [Fact]
    public void NotificationChannelKind_Lists_Planned_Channels_Only()
    {
        var names = Enum.GetNames<NotificationChannelKind>();
        Assert.Equal(3, names.Length);
        Assert.Contains("Email", names);
        Assert.Contains("Sms", names);
        Assert.Contains("InApp", names);
    }

    [Fact]
    public void Notification_T005_Forbids_Provider_And_Channel_Persistence_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(EmailProvider|SmsProvider|PushProvider|INotificationProvider|ISmtpClient|SmtpClient|TwilioClient|NotificationDelivery|DeliveryAttempt|ChannelPreference|NotificationTemplate)\b",
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

        Assert.True(hits.Count == 0, "Notification T005 forbids provider/persistence product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Notification_T005_Has_No_New_Migration_Or_Product_Table_Additions()
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
        Assert.False(NotificationChannelBoundary.ChannelPersistenceImplemented);
    }

    [Fact]
    public void P25_Evidence_Records_T005_And_R2()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P25-implementation-plan.md"));
        Assert.Contains("TC-P25-T005", plan, StringComparison.Ordinal);
        Assert.Contains("P25-R2", plan, StringComparison.Ordinal);
        Assert.Contains("NotificationChannelBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
