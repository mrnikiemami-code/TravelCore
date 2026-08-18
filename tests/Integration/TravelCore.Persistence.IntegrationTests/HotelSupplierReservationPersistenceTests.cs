using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Reservations;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelSupplierReservationPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelSupplierReservationPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Complete_Source_Confirms_Booking_Without_Payment()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        var source = new FakeHotelReservationSource();
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            bookingId = booking.Id;
            source.NextCreate = Complete(booking, booking.Rooms.Select(r => r.Id.Value).ToArray(), 1_000_000m);
            var service = CreateService(db, source);
            var reservation = await service.InitiateAsync(booking.Id, "key-1", cancellationToken: ct);
            Assert.Equal(HotelSupplierReservationStatus.Confirmed, reservation.Status);
            Assert.Equal(1, source.CreateCalls);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.HotelBookings.SingleAsync(x => x.Id == bookingId, ct);
            Assert.Equal(HotelBookingStatus.Confirmed, loaded.Status);
            Assert.NotNull(loaded.ConfirmedAt);
            Assert.Equal(0, await db.HotelBookingReconciliationIssues.CountAsync(x => x.HotelBookingId == bookingId, ct));
        }
    }

    [Fact]
    public async Task Timeout_Leaves_Initiated_And_Blocks_Retry_Until_Failed()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        var timeoutSource = new FakeHotelReservationSource { CreateException = new TimeoutException("network") };
        HotelSupplierReservationId reservationId;
        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            bookingId = booking.Id;
            var service = CreateService(db, timeoutSource);
            var reservation = await service.InitiateAsync(booking.Id, "key-timeout", cancellationToken: ct);
            reservationId = reservation.Id;
            Assert.Equal(HotelSupplierReservationAttemptStatus.Initiated, reservation.Attempts.Single().Status);
            Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);
            Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var service = CreateService(db, new FakeHotelReservationSource());
            var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(bookingId, "key-retry", cancellationToken: ct));
            Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);

            var sameKey = await service.InitiateAsync(bookingId, "key-timeout", cancellationToken: ct);
            Assert.Equal(reservationId, sameKey.Id);
            Assert.Equal(HotelSupplierReservationAttemptStatus.Initiated, sameKey.Attempts.Single().Status);
        }
    }

    [Fact]
    public async Task Authoritative_Failed_Allows_New_Attempt_And_Partial_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            bookingId = booking.Id;
            var failed = new FakeHotelReservationSource
            {
                NextCreate = new HotelReservationSourceResult(
                    HotelReservationSourceOutcome.Failed,
                    null,
                    null,
                    [],
                    null,
                    null),
            };
            var service = CreateService(db, failed);
            var reservation = await service.InitiateAsync(booking.Id, "key-fail", cancellationToken: ct);
            Assert.Equal(HotelSupplierReservationAttemptStatus.Failed, reservation.Attempts.Single().Status);
            Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.HotelBookings.Include(x => x.Rooms).SingleAsync(x => x.Id == bookingId, ct);
            var firstRoom = booking.Rooms.OrderBy(r => r.Ordinal).First().Id.Value;
            var partial = new FakeHotelReservationSource
            {
                NextCreate = new HotelReservationSourceResult(
                    HotelReservationSourceOutcome.Partial,
                    $"src-partial-{booking.Id.Value:N}",
                    null,
                    [firstRoom],
                    new MoneyValue(1_000_000m, CurrencyCode.Parse("IRR")),
                    true),
            };
            var service = CreateService(db, partial);
            var reservation = await service.InitiateAsync(bookingId, "key-partial", cancellationToken: ct);
            Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);
            Assert.Equal(HotelSupplierReservationAttemptStatus.Initiated, reservation.Attempts.Single(a => a.IsUnresolved).Status);
            Assert.Contains(
                await db.HotelBookingReconciliationIssues.ToListAsync(ct),
                i => i.Kind == HotelBookingReconciliationIssueKind.RoomSetMismatch);

            var reloaded = await db.HotelBookings.SingleAsync(x => x.Id == bookingId, ct);
            Assert.Equal(HotelBookingStatus.Pending, reloaded.Status);
        }
    }

    [Fact]
    public async Task Monetary_Mismatch_Persists_Issue_And_Unconfigured_Source_Does_Not_Fabricate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            bookingId = booking.Id;
            var mismatch = new FakeHotelReservationSource
            {
                NextCreate = Complete(booking, booking.Rooms.Select(r => r.Id.Value).ToArray(), 1_250_000m),
            };
            var service = CreateService(db, mismatch);
            var reservation = await service.InitiateAsync(booking.Id, "key-mismatch", cancellationToken: ct);
            Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);
            Assert.Contains(
                await db.HotelBookingReconciliationIssues.ToListAsync(ct),
                i => i.Kind == HotelBookingReconciliationIssueKind.MonetaryMismatch);
            Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            var empty = new HotelReservationSourceResolver([]);
            var service = new HotelSupplierReservationService(db, empty, new FixedClock(T0));
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(booking.Id, "key-unconfigured", cancellationToken: ct));
            Assert.Contains("unconfigured", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, await db.HotelSupplierReservations.CountAsync(x => x.HotelBookingId == booking.Id, ct));
            Assert.Equal(1, await db.HotelSupplierReservations.CountAsync(x => x.HotelBookingId == bookingId, ct));
        }
    }

    [Fact]
    public async Task Requested_Hold_Does_Not_Call_Reservation_Source()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        await using var db = _postgres.CreateDbContext();
        var booking = CreateBooking();
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, "offer-hold", 1_000_000m));
        db.HotelAvailabilityHolds.Add(
            HotelAvailabilityHold.StartRequested(
                booking.Id,
                "test-source",
                T0,
                booking.Rooms.Select(r => r.Id).ToArray()));
        await db.SaveChangesAsync(ct);

        var source = new FakeHotelReservationSource();
        var service = CreateService(db, source);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.InitiateAsync(booking.Id, "key-hold", cancellationToken: ct));
        Assert.Contains("Active", ex.Message, StringComparison.Ordinal);
        Assert.Equal(0, source.CreateCalls);
        Assert.Equal(0, await db.HotelSupplierReservations.CountAsync(x => x.HotelBookingId == booking.Id, ct));
    }

    [Fact]
    public async Task Recheck_Converges_After_Local_Crash_And_One_Reservation_Per_Booking()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        HotelSupplierReservationId reservationId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            bookingId = booking.Id;
            var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
            var attempt = reservation.StartAttempt(T0);
            reservation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
            reservation.RecordSourceCorrelation($"src-crash-{booking.Id.Value:N}", "CNF-crash");
            db.HotelSupplierReservations.Add(reservation);
            await db.SaveChangesAsync(ct);
            reservationId = reservation.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.HotelBookings.Include(x => x.Rooms).SingleAsync(x => x.Id == bookingId, ct);
            var source = new FakeHotelReservationSource
            {
                NextQuery = new HotelReservationQueryResult(
                    HotelReservationQueryStatus.Confirmed,
                    $"src-crash-{booking.Id.Value:N}",
                    booking.Rooms.Select(r => r.Id.Value).ToArray(),
                    new MoneyValue(1_000_000m, CurrencyCode.Parse("IRR"))),
            };
            var service = CreateService(db, source);
            var rechecked = await service.RecheckAsync(reservationId, ct);
            Assert.Equal(HotelSupplierReservationStatus.Confirmed, rechecked.Status);
            var loaded = await db.HotelBookings.SingleAsync(x => x.Id == bookingId, ct);
            Assert.Equal(HotelBookingStatus.Confirmed, loaded.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            db.HotelSupplierReservations.Add(HotelSupplierReservation.StartPending(booking.Id, "test-source", T0));
            await db.SaveChangesAsync(ct);
            db.HotelSupplierReservations.Add(HotelSupplierReservation.StartPending(booking.Id, "other-source", T0));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedPendingBookingWithOfferAndHoldAsync(db, ct);
            var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
            reservation.StartAttempt(T0);
            db.HotelSupplierReservations.Add(reservation);
            await db.SaveChangesAsync(ct);

            await using var other = _postgres.CreateDbContext();
            var loaded = await other.HotelSupplierReservations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.Id == reservation.Id, ct);
            Assert.Throws<InvalidOperationException>(
                () => loaded.StartAttempt(T0.Plus(Duration.FromSeconds(2))));

            var concurrentAttemptId = Guid.Parse("0198b3e0-0000-7000-8000-00000000d201");
            var unique = await Record.ExceptionAsync(() => other.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO hotel_booking.hotel_supplier_reservation_attempts
                    (id, hotel_supplier_reservation_id, status, created_at)
                VALUES
                    ({0}, {1}, 1, TIMESTAMPTZ '2026-08-18 12:00:00+00');
                """,
                concurrentAttemptId,
                reservation.Id.Value));
            Assert.NotNull(unique);
            Assert.Contains(
                "ux_hotel_supplier_reservation_attempts_one_unresolved",
                unique.Message,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Existing_Rows_Default_To_Pending_And_Wrong_Table_Names_Stay_Absent()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        await using var db = _postgres.CreateDbContext();
        var id = Guid.Parse("0198b3e0-0000-7000-8000-00000000d101");
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO hotel_booking.hotel_bookings
                (id, place_id, check_in_date, check_out_date)
            VALUES
                ({0}, {1}, DATE '2026-08-18', DATE '2026-08-19');
            """,
            id,
            Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));

        var conn = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        Assert.Equal(1, await ScalarIntAsync(conn, $"""
            SELECT status::int FROM hotel_booking.hotel_bookings WHERE id = '{id}';
            """, ct));
        Assert.Equal(1, await ScalarIntAsync(conn, """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = 'hotel_booking'
              AND table_name = 'hotel_supplier_reservations';
            """, ct));
        Assert.Equal(0, await ScalarIntAsync(conn, """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = 'hotel_booking'
              AND table_name IN (
                    'supplier_reservations',
                    'hotel_quotes',
                    'hotel_booking_payments');
            """, ct));
    }

    private static HotelSupplierReservationService CreateService(
        HotelBookingDbContext db,
        IHotelReservationSource source) =>
        new(db, new HotelReservationSourceResolver([source]), new FixedClock(T0));

    private static async Task<Stay> SeedPendingBookingWithOfferAndHoldAsync(
        HotelBookingDbContext db,
        CancellationToken cancellationToken)
    {
        var booking = CreateBooking();
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, "offer-res", 1_000_000m));
        var hold = HotelAvailabilityHold.StartRequested(
            booking.Id,
            "test-source",
            T0,
            booking.Rooms.Select(r => r.Id).ToArray());
        hold.Activate(
            T0.Plus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            $"src-hold-{booking.Id.Value:N}",
            booking.Rooms.ToDictionary(r => r.Id, r => $"sel-{r.Ordinal}"));
        db.HotelAvailabilityHolds.Add(hold);
        await db.SaveChangesAsync(cancellationToken);
        return booking;
    }

    private static Stay CreateBooking() =>
        Stay.Create(
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

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking, string offerRef, decimal total)
    {
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        return HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            $"offer-{booking.Id.Value:N}-{offerRef}",
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            new MoneyValue(total, irr),
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, irr), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600_000m, irr), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, new MoneyValue(total, irr)),
            ],
            propertyTimeZoneId: "Asia/Tehran");
    }

    private static HotelReservationSourceResult Complete(Stay booking, Guid[] roomIds, decimal amount) =>
        new(
            HotelReservationSourceOutcome.Complete,
            $"src-res-{booking.Id.Value:N}",
            "CNF-1",
            roomIds,
            new MoneyValue(amount, CurrencyCode.Parse("IRR")),
            true);

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

    private sealed class FakeHotelReservationSource : IHotelReservationSource
    {
        public ReservationSourceKey Key { get; } = new("test-source");

        public bool RequiresActiveHold { get; set; } = true;

        public bool NotFoundProvesNoReservation { get; set; }

        public HotelReservationSourceResult? NextCreate { get; set; }

        public Exception? CreateException { get; set; }

        public HotelReservationQueryResult? NextQuery { get; set; }

        public int CreateCalls { get; private set; }

        public Task<HotelReservationSourceResult> CreateReservationAsync(
            HotelReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return Task.FromResult(NextCreate
                ?? throw new InvalidOperationException("NextCreate is required."));
        }

        public Task<HotelReservationQueryResult> QueryReservationStatusAsync(
            string sourceReservationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NextQuery ?? new HotelReservationQueryResult(
                HotelReservationQueryStatus.PendingUnknown,
                sourceReservationReference,
                [],
                null));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
