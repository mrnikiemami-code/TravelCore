using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Place.Infrastructure;

/// <summary>
/// Explicit Place-owned migrator. Not auto-run on host startup.
/// </summary>
public static class PlaceMigrator
{
    public static Task MigrateAsync(
        PlaceDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
