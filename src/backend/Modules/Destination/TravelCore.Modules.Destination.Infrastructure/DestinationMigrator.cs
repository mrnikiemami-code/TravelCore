using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Explicit Destination-owned migrator. Not auto-run on host startup.
/// </summary>
public static class DestinationMigrator
{
    public static Task MigrateAsync(
        DestinationDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
