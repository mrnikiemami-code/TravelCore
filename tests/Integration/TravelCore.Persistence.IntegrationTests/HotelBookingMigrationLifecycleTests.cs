using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL HotelBooking schema scaffolding smoke (TC-P21-T001).
/// </summary>
[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelBookingMigrationLifecycleTests
{
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelBookingMigrationLifecycleTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task HotelBookingMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialHotelBookingScaffolding", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'hotel_booking';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name IN (
                        'hotel_bookings',
                        'hotel_booking_rooms',
                        'hotel_booking_guests',
                        'hotel_booking_holds',
                        'supplier_reservations',
                        'hotel_rates');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'hotel_booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN (
                        'place', 'booking', 'payment', 'pricing', 'tour',
                        'party', 'identity', 'agency_marketplace', 'search');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("hotel_booking", db.Model.GetDefaultSchema());
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
