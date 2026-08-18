using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelAvailabilityHoldPersistenceTests
{
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelAvailabilityHoldPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Hold_RoundTrips_Requested_Then_Active_Without_Peer_Fk_Or_Price()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        var now = Instant.FromUtc(2026, 8, 18, 12, 0);
        HotelAvailabilityHoldId holdId;

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = Stay.Create(
                new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021")),
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 20),
                HotelBookingContactSnapshot.Create(email: "lead@example.com"),
                [
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                    ]),
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, false),
                    ]),
                ]);
            db.HotelBookings.Add(booking);
            var hold = HotelAvailabilityHold.StartRequested(
                booking.Id,
                "test-source",
                now,
                booking.Rooms.Select(r => r.Id).ToArray());
            hold.Activate(
                now.Plus(Duration.FromMinutes(1)),
                now.Plus(Duration.FromHours(2)),
                "src-hold-1",
                booking.Rooms.ToDictionary(r => r.Id, r => $"sel-{r.Ordinal}"));
            db.HotelAvailabilityHolds.Add(hold);
            holdId = hold.Id;
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.HotelAvailabilityHolds
                .Include(x => x.Rooms)
                .SingleAsync(x => x.Id == holdId, ct);
            Assert.Equal(HotelAvailabilityHoldStatus.Active, loaded.Status);
            Assert.Equal(2, loaded.Rooms.Count);
            Assert.Equal("src-hold-1", loaded.SourceHoldReference);
            Assert.NotNull(loaded.ExpiresAt);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'hotel_booking'
                  AND table_name IN ('hotel_availability_holds', 'hotel_availability_hold_rooms')
                  AND (column_name LIKE '%price%' OR column_name LIKE '%amount%' OR column_name = 'rate_plan_id');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }
}
