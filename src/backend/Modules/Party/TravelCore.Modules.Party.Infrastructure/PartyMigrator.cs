using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Party.Infrastructure;

/// <summary>
/// Explicit Party-owned migrator. Not auto-run on host startup.
/// </summary>
public static class PartyMigrator
{
    public static Task MigrateAsync(
        PartyDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
