using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Search.Infrastructure;

/// <summary>
/// Explicit Search-owned migrator. Not auto-run on host startup.
/// </summary>
public static class SearchMigrator
{
    public static Task MigrateAsync(
        SearchDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
