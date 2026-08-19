using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.B2B.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL B2B schema foundation (TC-P24-T001). Schema b2b only; no product tables; no peer FK.
/// </summary>
[Collection(nameof(B2BMigrationLifecycleCollection))]
public sealed class B2BMigrationLifecycleTests
{
    private readonly B2BMigrationLifecycleContainerFixture _postgres;

    public B2BMigrationLifecycleTests(B2BMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task B2BMigrationLifecycle_Apply_SchemaFoundation()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_InitialB2BScaffolding", StringComparison.Ordinal));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await B2BMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'b2b';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'b2b'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'b2b'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'b2b'
                  AND ccu.table_schema <> 'b2b';
                """, ct));

            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
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
