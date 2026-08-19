using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Notification.Infrastructure;

/// <summary>
/// Explicit Notification-owned migrator. Not auto-run on host startup.
/// </summary>
public static class NotificationMigrator
{
    public static Task MigrateAsync(
        NotificationDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
