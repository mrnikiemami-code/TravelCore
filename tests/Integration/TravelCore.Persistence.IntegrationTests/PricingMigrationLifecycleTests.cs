using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Pricing schema + Price/Quote + occupancy + requested display currency (T001–T007).
/// T008 is a public read query only — no new tables.
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
            Assert.Equal(5, expectedMigrations.Length);
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_InitialPricingScaffolding", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddPriceAndPriceComponents", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddQuoteAndPriceSnapshot", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddPriceOccupancyRules", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddQuoteRequestedDisplayCurrency", StringComparison.Ordinal));
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
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = 'price_occupancy_rules';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = 'quotes';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name = 'quote_snapshot_components';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'pricing'
                  AND table_name = 'quotes'
                  AND column_name = 'requested_display_currency';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'pricing'
                  AND table_name = 'quotes'
                  AND column_name IN ('converted_amount', 'display_amount', 'exchange_rate', 'fx_rate');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'pricing'
                  AND table_name IN ('exchange_rates', 'fx_rates', 'payments', 'settlements');
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
