using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Media.Infrastructure;

/// <summary>
/// Explicit Media-owned migrator. Not auto-run on host startup.
/// </summary>
public static class MediaMigrator
{
    public static Task MigrateAsync(
        MediaDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
