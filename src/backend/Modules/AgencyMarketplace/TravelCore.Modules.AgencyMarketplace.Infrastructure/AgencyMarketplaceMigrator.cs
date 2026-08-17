using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Explicit Agency Marketplace-owned migrator. Not auto-run on host startup.
/// </summary>
public static class AgencyMarketplaceMigrator
{
    public static Task MigrateAsync(
        AgencyMarketplaceDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
