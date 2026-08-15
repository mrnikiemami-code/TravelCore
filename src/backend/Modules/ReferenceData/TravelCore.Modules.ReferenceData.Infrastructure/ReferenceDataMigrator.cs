using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.ReferenceData.Infrastructure.Seeding;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// Explicit ReferenceData-owned migrator. Not auto-run on host startup.
/// </summary>
public static class ReferenceDataMigrator
{
    public static async Task MigrateAsync(
        ReferenceDataDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        await context.Database.MigrateAsync(cancellationToken);
        await ReferenceDataCatalogSeeder.EnsureSeededAsync(context, cancellationToken);
    }
}
