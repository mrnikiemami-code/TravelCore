using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.CommercialFinance.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Commercial Finance schema + persistence foundation (TC-P39-T006).
/// </summary>
[Collection(nameof(CommercialFinanceMigrationLifecycleCollection))]
public sealed class CommercialFinanceMigrationLifecycleTests
{
    private readonly CommercialFinanceMigrationLifecycleContainerFixture _postgres;

    public CommercialFinanceMigrationLifecycleTests(CommercialFinanceMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task CommercialFinanceMigrationLifecycle_Apply_PersistenceFoundation()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_InitialCommercialFinanceScaffolding", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_P39CommercialFinancePersistenceFoundation", StringComparison.Ordinal));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await CommercialFinanceMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'commercial_finance';
                """, ct));

            foreach (var table in new[]
                     {
                         "commission_agreements",
                         "agency_offer_commission_overrides",
                         "commercial_obligations",
                         "settlement_periods",
                         "settlement_records",
                         "payout_instructions",
                         "event_consumption_records",
                     })
            {
                Assert.Equal(1, await ScalarIntAsync(conn, $"""
                    SELECT COUNT(*)::int
                    FROM information_schema.tables
                    WHERE table_schema = 'commercial_finance'
                      AND table_name = '{table}';
                    """, ct));
            }

            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'commercial_finance'
                  AND ccu.table_schema <> 'commercial_finance';
                """, ct));

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'commercial_finance'
                  AND table_name = 'agency_offer_commission_overrides'
                  AND column_name = 'agency_offer_id';
                """, ct));

            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'commercial_finance'
                  AND table_name = 'commission_agreements'
                  AND column_name IN ('commission_rate', 'commission_percent', 'formula');
                """, ct));

            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.Equal("commercial_finance", db.Model.GetDefaultSchema());
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}
