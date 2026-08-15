using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Identity.Infrastructure;

/// <summary>
/// Explicit Identity-owned migrator. Not auto-run on host startup.
/// </summary>
public static class IdentityMigrator
{
    public static Task MigrateAsync(
        IdentityDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
