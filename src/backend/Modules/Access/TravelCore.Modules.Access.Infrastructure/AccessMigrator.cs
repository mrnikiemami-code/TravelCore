using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Access.Infrastructure;

public static class AccessMigrator
{
    public static Task MigrateAsync(AccessDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
