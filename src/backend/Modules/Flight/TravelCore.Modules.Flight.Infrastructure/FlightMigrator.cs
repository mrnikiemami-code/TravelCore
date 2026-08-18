using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Explicit Flight-owned migrator. Not auto-run on host startup.
/// </summary>
public static class FlightMigrator
{
    public static Task MigrateAsync(
        FlightDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
