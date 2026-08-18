using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;
using HotelBookingAggregate = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelBookingStayPersistenceTests
{
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelBookingStayPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task HotelBooking_RoundTrips_MultiRoom_Stay_Without_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        var place = new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));
        var checkIn = new LocalDate(2026, 8, 18);
        var checkOut = new LocalDate(2026, 8, 21);
        HotelBookingId id;

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = HotelBookingAggregate.Create(
                place,
                checkIn,
                checkOut,
                HotelBookingContactSnapshot.Create(email: "lead@example.com", phone: "+989121234567"),
                [
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, isLeadGuest: true),
                        new HotelBookingGuestSpecification("Ann", "Lovelace", HotelGuestCategory.Child, isLeadGuest: false, ageAtCheckInYears: 8),
                    ]),
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, isLeadGuest: false),
                    ]),
                ]);
            id = booking.Id;
            db.HotelBookings.Add(booking);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.HotelBookings
                .Include(x => x.Rooms)
                .ThenInclude(r => r.Guests)
                .SingleAsync(x => x.Id == id, ct);

            Assert.Equal(place, loaded.Place);
            Assert.Equal(checkIn, loaded.CheckInDate);
            Assert.Equal(checkOut, loaded.CheckOutDate);
            Assert.Equal(3, loaded.Nights);
            Assert.Equal(2, loaded.RoomCount);
            Assert.Equal(3, loaded.GuestCount);
            Assert.Equal(2, loaded.AdultCount);
            Assert.Equal(1, loaded.ChildCount);
            Assert.Equal("lead@example.com", loaded.Contact.Email);
            Assert.Equal("+989121234567", loaded.Contact.Phone);

            var rooms = loaded.Rooms.OrderBy(r => r.Ordinal).ToArray();
            Assert.Equal(1, rooms[0].Ordinal);
            Assert.Equal(2, rooms[0].GuestCount);
            Assert.Equal(1, rooms[0].AdultCount);
            Assert.Equal(1, rooms[0].ChildCount);
            Assert.Equal(1, rooms[1].GuestCount);
            Assert.Equal(loaded.LeadGuest.Id, rooms[0].Guests.Single(g => g.IsLeadGuest).Id);
            Assert.Equal(HotelBookingStatus.Pending, loaded.Status);
            Assert.Null(loaded.ConfirmedAt);
            Assert.All(loaded.Guests, g => Assert.Contains(g.RoomReservationId, rooms.Select(r => r.Id)));
            Assert.Equal(8, rooms[0].Guests.Single(g => g.Category == HotelGuestCategory.Child).AgeAtCheckIn!.Value.Years);
            Assert.Null(loaded.LeadGuest.AgeAtCheckIn);
            Assert.Equal(7, loaded.Id.Value.Version);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_bookings'
                  AND column_name IN ('id', 'place_id', 'check_in_date', 'check_out_date', 'status');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'hotel_booking'
                  AND table_name IN ('room_reservations', 'hotel_booking_guests')
                  AND (
                        column_name IN (
                            'nights', 'status', 'rate_plan_id', 'rate_offer_id', 'quote_id',
                            'payment_id', 'supplier_reservation_id', 'birth_date', 'passport')
                        OR column_name LIKE '%price%'
                        OR column_name LIKE '%amount%');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_bookings'
                  AND (
                        column_name IN (
                            'nights', 'rate_plan_id', 'rate_offer_id', 'quote_id',
                            'payment_id', 'supplier_reservation_id', 'birth_date', 'passport')
                        OR column_name LIKE '%price%'
                        OR column_name LIKE '%amount%');
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
        }
    }

    [Fact]
    public async Task Invalid_Stay_Dates_And_Second_Lead_Are_Rejected_By_Database()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var sameDay = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO hotel_booking.hotel_bookings
                    (id, place_id, check_in_date, check_out_date)
                VALUES
                    ({0}, {1}, DATE '2026-08-18', DATE '2026-08-18');
                """,
                Guid.Parse("0198b3e0-0000-7000-8000-00000000d001"),
                Guid.Parse("0198b3e0-0000-7000-8000-000000000021")));
            Assert.NotNull(sameDay);
            Assert.Contains("ck_hotel_bookings_checkout_after_checkin", sameDay.Message, StringComparison.OrdinalIgnoreCase);

            var reversed = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO hotel_booking.hotel_bookings
                    (id, place_id, check_in_date, check_out_date)
                VALUES
                    ({0}, {1}, DATE '2026-08-19', DATE '2026-08-18');
                """,
                Guid.Parse("0198b3e0-0000-7000-8000-00000000d002"),
                Guid.Parse("0198b3e0-0000-7000-8000-000000000021")));
            Assert.NotNull(reversed);
            Assert.Contains("ck_hotel_bookings_checkout_after_checkin", reversed.Message, StringComparison.OrdinalIgnoreCase);
        }

        HotelBookingId id;
        RoomReservationId roomId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = HotelBookingAggregate.Create(
                new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021")),
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                HotelBookingContactSnapshot.Create(phone: "+989121234567"),
                [
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, isLeadGuest: true),
                    ]),
                ]);
            id = booking.Id;
            roomId = booking.Rooms[0].Id;
            db.HotelBookings.Add(booking);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var secondLead = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO hotel_booking.hotel_booking_guests
                    (id, hotel_booking_id, room_reservation_id, given_name, family_name, category, age_at_check_in, is_lead_guest)
                VALUES
                    ({0}, {1}, {2}, 'Alan', 'Turing', 1, NULL, TRUE);
                """,
                Guid.Parse("0198b3e0-0000-7000-8000-00000000d003"),
                id.Value,
                roomId.Value));
            Assert.NotNull(secondLead);
            Assert.Contains("ux_hotel_booking_guests_one_lead", secondLead.Message, StringComparison.OrdinalIgnoreCase);
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
