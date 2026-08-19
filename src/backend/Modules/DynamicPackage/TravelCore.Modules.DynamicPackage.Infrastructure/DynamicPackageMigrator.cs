using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.DynamicPackage.Infrastructure;

/// <summary>
/// Explicit DynamicPackage-owned migrator. Not auto-run on host startup.
/// </summary>
public static class DynamicPackageMigrator
{
    public static Task MigrateAsync(
        DynamicPackageDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
