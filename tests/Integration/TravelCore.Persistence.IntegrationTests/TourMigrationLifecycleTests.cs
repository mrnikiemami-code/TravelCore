using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Tour.Domain;
using TravelCore.Modules.Tour.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Tour migration + TourProduct shared-core smoke (TC-P09-T002 / P09-R1 / P09-R7).
/// </summary>
[Collection(nameof(TourMigrationLifecycleCollection))]
public sealed class TourMigrationLifecycleTests
{
    private readonly TourMigrationLifecycleContainerFixture _postgres;

    public TourMigrationLifecycleTests(TourMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task TourMigrationLifecycle_Apply_And_Persist_TourProduct_EndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialTourScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductTables", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await TourMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'tour';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'tour'
                  AND table_name IN ('tour_products', '__EFMigrationsHistory');
                """, ct));
            // P09-R7: no specialty / departure tables in P09 T002.
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'tour'
                  AND table_name IN (
                    'experience_tours',
                    'package_tours',
                    'tour_departures',
                    'flight_segments',
                    'tour_hotel_options');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        var now = Instant.FromUtc(2026, 8, 17, 2, 30);
        TourProductId createdId;

        await using (var db = _postgres.CreateDbContext())
        {
            var experience = TourProduct.CreateExperience("EXP-IT-001", "Caspian Walk", now);
            var package = TourProduct.CreatePackage("PKG-IT-001", "Istanbul Package", now);
            createdId = experience.Id;
            db.TourProducts.AddRange(experience, package);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.TourProducts.SingleAsync(x => x.Id == createdId, ct);
            Assert.Equal(TourKind.Experience, loaded.Kind);
            Assert.Equal("EXP-IT-001", loaded.Code);
            Assert.Equal("Caspian Walk", loaded.EnglishName);

            var package = await db.TourProducts.SingleAsync(x => x.Code == "PKG-IT-001", ct);
            Assert.Equal(TourKind.Package, package.Kind);
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
