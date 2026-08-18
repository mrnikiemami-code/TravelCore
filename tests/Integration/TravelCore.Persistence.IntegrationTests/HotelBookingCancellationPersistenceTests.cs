using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Reservations;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelBookingCancellationPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelBookingCancellationPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Full_Refund_Path_Commits_Cancellation_Outbox_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        await using (var db = _postgres.CreateDbContext())
        {
            var seed = await SeedConfirmedAsync(db, ct);
            bookingId = seed.Booking.Id;
            var result = await CreateService(db, source).RequestAsync(seed.Booking.Id, "cancel-1", ct);
            Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, result.Outcome);
            Assert.Equal(1, source.CancelCalls);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.HotelBookings.SingleAsync(x => x.Id == bookingId, ct);
            var reservation = await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == bookingId, ct);
            var cancellation = await db.HotelBookingCancellations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.HotelBookingId == bookingId, ct);
            Assert.Equal(HotelBookingStatus.Cancelled, booking.Status);
            Assert.Equal(HotelSupplierReservationStatus.Cancelled, reservation.Status);
            Assert.Equal(HotelBookingCancellationStatus.RefundPending, cancellation.Status);
            Assert.Equal(
                HotelSupplierCancellationAttemptStatus.Confirmed,
                cancellation.Attempts.Single().Status);
            Assert.Equal(
                1,
                await db.OutboxMessages.CountAsync(
                    x => x.Id == cancellation.Id.Value
                        && x.MessageType == HotelBookingCancellationRefundOutboxBoundary.MessageType,
                    ct));
        }
    }

    [Fact]
    public async Task Concurrent_Requests_Create_One_Cancellation_And_Unconfigured_Does_Not_Fabricate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var seed = await SeedConfirmedAsync(db, ct);
            bookingId = seed.Booking.Id;
        }

        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        await using (var dbA = _postgres.CreateDbContext())
        await using (var dbB = _postgres.CreateDbContext())
        {
            var serviceA = CreateService(dbA, source);
            var serviceB = CreateService(dbB, source);
            var first = serviceA.RequestAsync(bookingId, "key-a", ct);
            var second = serviceB.RequestAsync(bookingId, "key-b", ct);
            await Task.WhenAll(first, second);
            Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, first.Result.Outcome);
            Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, second.Result.Outcome);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(1, await db.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == bookingId, ct));
            var cancellation = await db.HotelBookingCancellations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.HotelBookingId == bookingId, ct);
            Assert.Equal(1, cancellation.Attempts.Count);
        }

        await using (var noneDb = _postgres.CreateDbContext())
        {
            var other = await SeedConfirmedAsync(noneDb, ct);
            var none = new HotelBookingCancellationService(
                noneDb,
                new HotelReservationSourceResolver([]),
                new FixedClock(T0));
            var result = await none.RequestAsync(other.Booking.Id, "none", ct);
            Assert.Equal(HotelBookingCancellationRequestOutcome.UnconfiguredReservationSource, result.Outcome);
            Assert.Equal(0, await noneDb.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == other.Booking.Id, ct));
            Assert.Equal(HotelBookingStatus.Confirmed, (await noneDb.HotelBookings.SingleAsync(x => x.Id == other.Booking.Id, ct)).Status);
        }
    }

    [Fact]
    public async Task Existing_Confirmed_Rows_Are_Not_Backfilled_And_Tables_Exist()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var seed = await SeedConfirmedAsync(db, ct);
            bookingId = seed.Booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(0, await db.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == bookingId, ct));
            Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == bookingId, ct)).Status);
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_booking_cancellations';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_supplier_cancellation_attempts';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'hotel_booking'
                  AND table_name = 'hotel_booking_cancellation_idempotency';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'hotel_booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('payment', 'booking', 'place');
                """, ct));
        }
    }

    private static HotelBookingCancellationService CreateService(
        HotelBookingDbContext db,
        IHotelReservationSource source) =>
        new(db, new HotelReservationSourceResolver([source]), new FixedClock(T0));

    private static async Task<ConfirmedSeed> SeedConfirmedAsync(
        HotelBookingDbContext db,
        CancellationToken cancellationToken)
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
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        var snapshot = HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            new MoneyValue(1_000_000m, irr),
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, irr), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600_000m, irr), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, new MoneyValue(1_000_000m, irr)),
            ],
            propertyTimeZoneId: "Asia/Tehran");
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            $"src-res-{booking.Id.Value:N}",
            "CNF-1",
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());
        var paymentId = Guid.CreateVersion7();
        var evidence = HotelBookingPaymentEvidence.Record(booking.Id, paymentId, 1_000_000m, "IRR", T0);
        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation,
            evidence,
            T0.Plus(Duration.FromMinutes(2)),
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            snapshot.Monetary.Total,
            true,
            snapshot.Monetary,
            []);
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(snapshot);
        db.HotelSupplierReservations.Add(reservation);
        db.HotelBookingPaymentEvidence.Add(evidence);
        await db.SaveChangesAsync(cancellationToken);
        return new ConfirmedSeed(booking, paymentId);
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

    private sealed record ConfirmedSeed(Stay Booking, Guid PaymentId);

    private sealed class FakeHotelReservationSource : IHotelReservationSource
    {
        public ReservationSourceKey Key { get; } = new("test-source");
        public bool RequiresActiveHold => false;
        public bool NotFoundProvesNoReservation => false;
        public HotelReservationCancellationSourceResult? NextCancel { get; set; }
        public int CancelCalls { get; private set; }

        public Task<HotelReservationSourceResult> CreateReservationAsync(
            HotelReservationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Create is not used.");

        public Task<HotelReservationQueryResult> QueryReservationStatusAsync(
            string sourceReservationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HotelReservationQueryResult(
                HotelReservationQueryStatus.Confirmed,
                sourceReservationReference,
                [],
                null));

        public Task<HotelReservationCancellationSourceResult> InitiateCancellationAsync(
            HotelReservationCancellationRequest request,
            CancellationToken cancellationToken = default)
        {
            CancelCalls++;
            return Task.FromResult(NextCancel
                ?? throw new InvalidOperationException("NextCancel is required."));
        }

        public Task<HotelReservationCancellationQueryResult> QueryCancellationStatusAsync(
            HotelReservationCancellationQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HotelReservationCancellationQueryResult(
                HotelReservationCancellationQueryStatus.PendingUnknown));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
