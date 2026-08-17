using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// Explicit UGC-owned migrator. Not auto-run on host startup.
/// </summary>
public static class UgcMigrator
{
    public static Task MigrateAsync(
        UgcDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
