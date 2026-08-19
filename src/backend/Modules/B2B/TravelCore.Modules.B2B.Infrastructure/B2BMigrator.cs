using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.B2B.Infrastructure;

/// <summary>
/// Explicit B2B-owned migrator. Not auto-run on host startup.
/// </summary>
public static class B2BMigrator
{
    public static Task MigrateAsync(
        B2BDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
