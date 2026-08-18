using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Booking.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Booking schema scaffolding smoke (TC-P19-T001).
/// </summary>
[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingMigrationLifecycleTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingMigrationLifecycleTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task BookingMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialBookingScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddBookingAggregateBaseline", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'booking';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'booking'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'booking'
                  AND table_name NOT IN ('__EFMigrationsHistory', 'bookings');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'booking'
                  AND table_name = 'bookings';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'booking'
                  AND table_name IN ('booking_passengers', 'booking_holds', 'booking_payments', 'booking_events');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('tour', 'pricing', 'party', 'identity', 'agency_marketplace', 'search', 'visa', 'trip_planner', 'payment');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("booking", db.Model.GetDefaultSchema());
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
