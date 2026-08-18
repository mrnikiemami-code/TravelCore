using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingAggregatePersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingAggregatePersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Booking_RoundTrips_Pending_Then_Cancelled_Without_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(migrate, ct);
        }

        var departure = new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000201"));
        var createdAt = Instant.FromUtc(2026, 8, 18, 4, 0);
        BookingId id;

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = Booking.Create(departure, createdAt);
            id = booking.Id;
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings.SingleAsync(x => x.Id == id, ct);
            Assert.Equal(BookingStatus.Pending, loaded.Status);
            Assert.Equal(departure, loaded.TourDeparture);
            Assert.Equal(createdAt, loaded.CreatedAt);

            loaded.CancelPending(Instant.FromUtc(2026, 8, 18, 5, 0));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings.SingleAsync(x => x.Id == id, ct);
            Assert.Equal(BookingStatus.Cancelled, loaded.Status);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'booking'
                  AND table_name = 'bookings'
                  AND column_name IN ('id', 'tour_departure_id', 'status', 'created_at', 'status_changed_at', 'source_kind');
                """;
            var required = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            Assert.Equal(6, required);

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'booking'
                  AND table_name = 'bookings'
                  AND column_name IN ('payment_id', 'quote_id', 'agency_id', 'passenger_json', 'reserved_seats', 'price_amount');
                """;
            var speculative = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            Assert.Equal(0, speculative);

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('tour', 'pricing', 'party', 'payment', 'agency_marketplace');
                """;
            var fks = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            Assert.Equal(0, fks);
        }
    }
}
