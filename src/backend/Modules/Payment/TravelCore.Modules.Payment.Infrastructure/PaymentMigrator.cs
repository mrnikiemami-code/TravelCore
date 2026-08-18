using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Explicit Payment-owned migrator. Not auto-run on host startup.
/// </summary>
public static class PaymentMigrator
{
    public static Task MigrateAsync(
        PaymentDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
