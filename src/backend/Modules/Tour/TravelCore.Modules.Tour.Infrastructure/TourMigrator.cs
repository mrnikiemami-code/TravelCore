using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Tour.Infrastructure;

/// <summary>
/// Explicit Tour-owned migrator. Not auto-run on host startup.
/// </summary>
public static class TourMigrator
{
    public static Task MigrateAsync(
        TourDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
