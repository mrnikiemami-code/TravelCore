using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL HotelBooking stay-structure smoke (TC-P21-T002).
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
    public async Task HotelBookingMigrationLifecycle_Apply_Stay_Structure()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(4, expectedMigrations.Length);
            Assert.EndsWith("_InitialHotelBookingScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddHotelBookingStayStructure", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddHotelAvailabilityHold", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddHotelRateOfferSnapshots", expectedMigrations[3], StringComparison.Ordinal);
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
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_bookings';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'room_reservations';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_booking_guests';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_availability_holds';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_availability_hold_rooms';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_hold_idempotency';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_rate_offer_snapshots';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_room_rate_snapshots';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_booking_monetary_snapshots';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_cancellation_policy_snapshots';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_cancellation_penalty_rules';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name IN (
                        'supplier_reservations',
                        'hotel_rates',
                        'hotel_quotes',
                        'hotel_booking_payments');
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
