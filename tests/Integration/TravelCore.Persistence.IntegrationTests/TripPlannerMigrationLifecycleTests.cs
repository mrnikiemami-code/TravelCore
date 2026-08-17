using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.TripPlanner.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL TripPlanner schema + TripIntent/Lead baseline (TC-P18-T002).
/// </summary>
[Collection(nameof(TripPlannerMigrationLifecycleCollection))]
public sealed class TripPlannerMigrationLifecycleTests
{
    private readonly TripPlannerMigrationLifecycleContainerFixture _postgres;

    public TripPlannerMigrationLifecycleTests(TripPlannerMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task TripPlannerMigrationLifecycle_Apply_TripIntent_And_Lead_Tables()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialTripPlannerScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddTripIntentLeadBaseline", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await TripPlannerMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'trip_planner';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'trip_planner'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'trip_planner'
                  AND table_name IN ('trip_intents', 'leads');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'trip_planner'
                  AND table_name IN ('lead_status_history', 'planner_contacts', 'travel_preferences');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'trip_planner'
                  AND table_name = 'leads'
                  AND column_name = 'captured_planning_revision';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'trip_planner'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('tour', 'destination', 'party', 'pricing', 'agency_marketplace', 'search', 'visa');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("trip_planner", db.Model.GetDefaultSchema());
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
