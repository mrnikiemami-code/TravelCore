using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL DynamicPackage schema foundation (TC-P23-T001). Schema dynamic_package only; no product tables; no peer FK.
/// </summary>
[Collection(nameof(DynamicPackageMigrationLifecycleCollection))]
public sealed class DynamicPackageMigrationLifecycleTests
{
    private readonly DynamicPackageMigrationLifecycleContainerFixture _postgres;

    public DynamicPackageMigrationLifecycleTests(DynamicPackageMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task DynamicPackageMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialDynamicPackageScaffolding", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await DynamicPackageMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'dynamic_package';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'dynamic_package'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'dynamic_package'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'dynamic_package'
                  AND table_name IN (
                    'dynamic_package_bookings',
                    'package_compositions',
                    'package_offers',
                    'package_sagas'
                  );
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'dynamic_package'
                  AND ccu.table_schema <> 'dynamic_package';
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
