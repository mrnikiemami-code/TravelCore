using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Ugc.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL UGC schema scaffolding smoke (TC-P16-T001).
/// </summary>
[Collection(nameof(UgcMigrationLifecycleCollection))]
public sealed class UgcMigrationLifecycleTests
{
    private readonly UgcMigrationLifecycleContainerFixture _postgres;

    public UgcMigrationLifecycleTests(UgcMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task UgcMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialUgcScaffolding", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await UgcMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'ugc';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name IN ('reviews', 'ratings', 'travelogues', 'comments', 'likes', 'reports');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'ugc'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('identity', 'party', 'tour', 'place', 'destination', 'content', 'media', 'seo', 'search');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("ugc", db.Model.GetDefaultSchema());
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
