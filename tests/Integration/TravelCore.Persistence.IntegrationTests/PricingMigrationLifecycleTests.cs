using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Pricing schema + Price tables (TC-P12-T001 / T003).
/// </summary>
[Collection(nameof(PricingMigrationLifecycleCollection))]
public sealed class PricingMigrationLifecycleTests
{
    private readonly PricingMigrationLifecycleContainerFixture _postgres;

    public PricingMigrationLifecycleTests(PricingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PricingMigrationLifecycle_Apply_Creates_Price_Tables_Without_Tour_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialPricingScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddPriceAndPriceComponents", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await PricingMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'pricing';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = 'prices';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = 'price_components';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'pricing'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema = 'tour';
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("pricing", db.Model.GetDefaultSchema());
        }
    }

    private static async Task<int> ScalarIntAsync(
        DbConnection conn,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}
