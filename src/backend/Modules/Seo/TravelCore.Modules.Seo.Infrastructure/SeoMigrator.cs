using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Seo.Infrastructure;

/// <summary>
/// Explicit SEO-owned migrator. Not auto-run on host startup.
/// </summary>
public static class SeoMigrator
{
    public static Task MigrateAsync(
        SeoDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
