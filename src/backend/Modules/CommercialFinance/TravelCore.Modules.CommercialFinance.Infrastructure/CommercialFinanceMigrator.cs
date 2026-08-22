using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.CommercialFinance.Infrastructure;

/// <summary>
/// Explicit Commercial Finance-owned migrator. Not auto-run on host startup.
/// </summary>
public static class CommercialFinanceMigrator
{
    public static Task MigrateAsync(
        CommercialFinanceDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
