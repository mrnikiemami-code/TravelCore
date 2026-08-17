using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.TripPlanner.Infrastructure;

/// <summary>
/// Explicit TripPlanner-owned migrator. Not auto-run on host startup.
/// </summary>
public static class TripPlannerMigrator
{
    public static Task MigrateAsync(
        TripPlannerDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
