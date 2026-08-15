using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.ReferenceData.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL ReferenceData migration + seed smoke (TC-P04-T002).
/// </summary>
[Collection(nameof(ReferenceDataMigrationLifecycleCollection))]
public sealed class ReferenceDataMigrationLifecycleTests
{
    private readonly ReferenceDataMigrationLifecycleContainerFixture _postgres;

    public ReferenceDataMigrationLifecycleTests(ReferenceDataMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task ReferenceDataMigrationLifecycle_Apply_Seed_And_ReadSmoke()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialReferenceDataPersistence", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await ReferenceDataMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'reference_data';
                """, ct));
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'reference_data'
                  AND table_name IN (
                    'currencies',
                    'locales',
                    'countries',
                    'time_zones',
                    '__EFMigrationsHistory');
                """, ct));

            Assert.Equal(3, await db.Currencies.CountAsync(ct));
            Assert.Equal(4, await db.Locales.CountAsync(ct));
            Assert.Equal(3, await db.Countries.CountAsync(ct));
            Assert.Equal(3, await db.TimeZones.CountAsync(ct));
            Assert.Contains(db.Currencies.AsEnumerable(), x => x.Code == "IRR");
            Assert.Contains(db.Countries.AsEnumerable(), x => x.Alpha2Code == "IR");
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        // Second migrate remains idempotent for seeds.
        await using (var db = _postgres.CreateDbContext())
        {
            await ReferenceDataMigrator.MigrateAsync(db, ct);
            Assert.Equal(3, await db.Currencies.CountAsync(ct));
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }
}
