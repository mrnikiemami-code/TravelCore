using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Explicit Content-owned migrator. Not auto-run on host startup.
/// </summary>
public static class ContentMigrator
{
    public static Task MigrateAsync(
        ContentDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
