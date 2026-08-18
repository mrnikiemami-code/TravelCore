using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingPeoplePersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingPeoplePersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Contact_And_Unicode_Passengers_RoundTrip_Without_Peer_Fk_Or_Passport_Columns()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 10, 0);
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            booking.SetContact(BookingContactSnapshot.Create("سارا نوری", "sara@example.com", "۰۹۱۲"));
            booking.SetActorReference(new BookingActorReference(Guid.CreateVersion7()));
            booking.AddPassenger("علی", "رضایی", TravelerCategory.Adult, activeHeldSeatCount: null);
            booking.AddPassenger("Maryam", "حسینی", TravelerCategory.Child, activeHeldSeatCount: null);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings.Include(x => x.Passengers).SingleAsync(x => x.Id == id, ct);
            Assert.Equal("سارا نوری", loaded.Contact!.DisplayName);
            Assert.Equal(2, loaded.PassengerCount);
            Assert.Contains(loaded.Passengers, p => p.GivenName == "علی");
            Assert.Contains(loaded.Passengers, p => p.GivenName == "Maryam");
            Assert.NotNull(loaded.ActorReference);
            Assert.Null(loaded.PartyReference);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'booking'
                  AND column_name IN ('passport_number', 'passport_expiry', 'national_id', 'document_uri', 'visa_number');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('party', 'identity', 'tour', 'pricing', 'payment');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    [Fact]
    public async Task Passenger_Count_Cannot_Exceed_Active_Hold_SeatCount()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 10, 30);
        var expires = now.Plus(Duration.FromMinutes(5));
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).HoldAsync(
                new HoldCapacityCommand(id, 1, 2, expires, now, "people-hold-" + id.Value),
                ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var people = new BookingPeopleService(db);
            await people.AddPassengerAsync(id, "Only", "One", TravelerCategory.Adult, ct);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => people.AddPassengerAsync(id, "Too", "Many", TravelerCategory.Adult, ct));
        }
    }
}
