using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Analytics.Infrastructure;

/// <summary>
/// Explicit Analytics-owned migrator. Not auto-run on host startup.
/// </summary>
public static class AnalyticsMigrator
{
    public static Task MigrateAsync(
        AnalyticsDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
