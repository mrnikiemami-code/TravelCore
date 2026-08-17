using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Explicit Pricing-owned migrator. Not auto-run on host startup.
/// </summary>
public static class PricingMigrator
{
    public static Task MigrateAsync(
        PricingDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
