using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Visa.Infrastructure;

/// <summary>
/// Explicit Visa-owned migrator. Not auto-run on host startup.
/// </summary>
public static class VisaMigrator
{
    public static Task MigrateAsync(
        VisaDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
