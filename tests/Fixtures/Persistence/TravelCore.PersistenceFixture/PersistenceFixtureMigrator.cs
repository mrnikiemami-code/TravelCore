using Microsoft.EntityFrameworkCore;

namespace TravelCore.PersistenceFixture;

/// <summary>
/// Explicit fixture-owned migrator (test/support). Not registered in TravelCore.Api.
/// Real PostgreSQL migration lifecycle acceptance: TC-P01-T017.
/// </summary>
public static class PersistenceFixtureMigrator
{
    public static Task MigrateAsync(
        PersistenceFixtureDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
