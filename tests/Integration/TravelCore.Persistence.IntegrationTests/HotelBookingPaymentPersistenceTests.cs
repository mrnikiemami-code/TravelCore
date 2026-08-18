using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Availability;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelBookingPaymentPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 16, 0);
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelBookingPaymentPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Payment_Success_Without_Supplier_Stays_Pending_And_Writes_Inbox_Evidence_Outbox()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedPendingAsync(ct);
        await HandlePaymentSuccessAsync(ctx, SuccessEvidence(ctx), ct);
        await HandlePaymentSuccessAsync(ctx, SuccessEvidence(ctx), ct);

        await using var db = _postgres.CreateDbContext();
        var booking = await db.HotelBookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Equal(1, await db.HotelBookingPaymentEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
        Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        Assert.Equal(
            1,
            await db.OutboxMessages.CountAsync(
                x => x.Id == ctx.BookingId.Value
                    && x.MessageType == HotelSupplierReservationRequiredOutboxBoundary.MessageType,
                ct));
        Assert.Equal(0, await db.HotelBookingReconciliationIssues.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
    }

    [Fact]
    public async Task Amount_Mismatch_Persists_Reconciliation_And_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedPendingAsync(ct);
        await HandlePaymentSuccessAsync(
            ctx,
            new FakeHotelPaymentEvidence
            {
                Next = new HotelBookingPaymentSuccessEvidenceRead(
                    ctx.PaymentId,
                    ctx.BookingId.Value,
                    "Succeeded",
                    999m,
                    "IRR",
                    true),
            },
            ct);

        await using var db = _postgres.CreateDbContext();
        Assert.Equal(HotelBookingStatus.Pending, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(0, await db.HotelBookingPaymentEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
        Assert.Contains(
            await db.HotelBookingReconciliationIssues.Where(x => x.HotelBookingId == ctx.BookingId).ToListAsync(ct),
            i => i.Kind == HotelBookingReconciliationIssueKind.MonetaryMismatch);
        Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        Assert.Equal(0, await db.PaymentCompensationEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
    }

    [Fact]
    public async Task Dual_Evidence_Confirms_Once()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedPendingAsync(ct, withConfirmedReservation: true);
        await HandlePaymentSuccessAsync(ctx, SuccessEvidence(ctx), ct);
        await HandlePaymentSuccessAsync(ctx, SuccessEvidence(ctx), ct);

        await using var db = _postgres.CreateDbContext();
        var booking = await db.HotelBookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        Assert.Equal(HotelBookingStatus.Confirmed, booking.Status);
        Assert.Equal(1, await db.HotelBookingPaymentEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
        Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
    }

    [Fact]
    public async Task Supplier_Only_Reservation_Leaves_Booking_Pending()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedPendingAsync(ct, withConfirmedReservation: true);

        await using var db = _postgres.CreateDbContext();
        var booking = await db.HotelBookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        var reservation = await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == ctx.BookingId, ct);
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Equal(HotelSupplierReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(0, await db.HotelBookingPaymentEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
    }

    [Fact]
    public async Task Compensation_Evidence_And_Outbox_Commit_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedPendingAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await HotelBookingPaymentRecovery.RecordCompensationAsync(
                db,
                ctx.BookingId,
                ctx.PaymentId,
                HotelBookingPaymentCompensationReason.HoldExpired,
                T0.Plus(Duration.FromMinutes(3)),
                ct);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(1, await db.PaymentCompensationEvidence.CountAsync(x => x.HotelBookingId == ctx.BookingId, ct));
            Assert.Equal(
                1,
                await db.OutboxMessages.CountAsync(
                    x => x.Id == ctx.PaymentId
                        && x.MessageType == HotelBookingCompensationOutboxBoundary.MessageType,
                    ct));
        }

        var rolledBack = await SeedPendingAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await HotelBookingPaymentRecovery.RecordCompensationAsync(
                db,
                rolledBack.BookingId,
                rolledBack.PaymentId,
                HotelBookingPaymentCompensationReason.HoldReleased,
                T0.Plus(Duration.FromMinutes(4)),
                ct);
            await db.SaveChangesAsync(ct);
            await tx.RollbackAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var evidence = await db.PaymentCompensationEvidence.SingleAsync(x => x.HotelBookingId == ctx.BookingId, ct);
            Assert.Equal(HotelBookingPaymentCompensationReason.HoldExpired, evidence.Reason);
            Assert.Equal(ctx.PaymentId, evidence.PaymentId);
            Assert.Equal(0, await db.PaymentCompensationEvidence.CountAsync(x => x.HotelBookingId == rolledBack.BookingId, ct));
            Assert.Equal(0, await db.OutboxMessages.CountAsync(x => x.Id == rolledBack.PaymentId, ct));
        }
    }

    [Fact]
    public async Task Refund_Success_Cancels_Pending_And_Does_Not_Cancel_Confirmed()
    {
        var ct = TestContext.Current.CancellationToken;
        var pending = await SeedPendingAsync(ct);
        await HandleRefundSuccessAsync(pending, ct);
        await HandleRefundSuccessAsync(pending, ct);

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(
                HotelBookingStatus.Cancelled,
                (await db.HotelBookings.SingleAsync(x => x.Id == pending.BookingId, ct)).Status);
            Assert.Equal(1, await db.RefundSuccessInbox.CountAsync(x => x.RefundId == pending.RefundId, ct));
        }

        var confirmed = await SeedPendingAsync(ct, withConfirmedReservation: true);
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.HotelBookings
                .Include(x => x.Rooms)
                .SingleAsync(x => x.Id == confirmed.BookingId, ct);
            var snapshot = await db.HotelRateOfferSnapshots
                .Include(x => x.Monetary)
                .SingleAsync(x => x.HotelBookingId == confirmed.BookingId, ct);
            var reservation = await db.HotelSupplierReservations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.HotelBookingId == confirmed.BookingId, ct);
            var evidence = HotelBookingPaymentEvidence.Record(
                confirmed.BookingId, confirmed.PaymentId, 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(1)));
            db.HotelBookingPaymentEvidence.Add(evidence);
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation,
                evidence,
                T0.Plus(Duration.FromMinutes(2)),
                snapshot.Place,
                snapshot.CheckInDate,
                snapshot.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(),
                snapshot.Monetary.Total,
                true,
                snapshot.Monetary,
                []);
            await db.SaveChangesAsync(ct);
        }

        await HandleRefundSuccessAsync(confirmed, ct);
        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(
                HotelBookingStatus.Confirmed,
                (await db.HotelBookings.SingleAsync(x => x.Id == confirmed.BookingId, ct)).Status);
            Assert.Contains(
                await db.RefundInvariantIssues.Where(x => x.HotelBookingId == confirmed.BookingId).ToListAsync(ct),
                i => i.Kind == HotelBookingRefundInvariantIssueKind.ConfirmedBooking);
        }
    }

    [Fact]
    public async Task Payment_Integration_Tables_Exist_Without_Peer_Schema_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = _postgres.CreateDbContext();
        await HotelBookingMigrator.MigrateAsync(db, ct);
        var conn = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = 'hotel_booking'
              AND table_name IN (
                    'hotel_booking_payment_evidence',
                    'payment_success_inbox',
                    'refund_success_inbox',
                    'hotel_booking_payment_compensation_evidence',
                    'outbox_messages');
            """;
        Assert.Equal(5, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

        cmd.CommandText = """
            SELECT COUNT(*)::int
            FROM information_schema.table_constraints tc
            JOIN information_schema.constraint_column_usage ccu
              ON tc.constraint_name = ccu.constraint_name
             AND tc.table_schema = ccu.table_schema
            WHERE tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_schema = 'hotel_booking'
              AND ccu.table_schema <> 'hotel_booking';
            """;
        Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
    }

    private async Task HandlePaymentSuccessAsync(
        SeededHotel ctx,
        IPaymentSuccessEvidenceQuery evidence,
        CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var handler = new HotelBookingPaymentSucceededIntegrationHandler(
            db,
            evidence,
            new FixedClock(T0.Plus(Duration.FromMinutes(5))));
        await handler.HandleAsync(
            new HotelBookingPaymentSucceededIntegrationEvent(
                ctx.PaymentId,
                ctx.BookingId.Value,
                T0.Plus(Duration.FromMinutes(4)),
                1_000_000m,
                "IRR"),
            ct);
    }

    private async Task HandleRefundSuccessAsync(SeededHotel ctx, CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var handler = new HotelBookingRefundSucceededIntegrationHandler(
            db,
            new HotelAvailabilityHoldService(
                db,
                new HotelAvailabilitySourceResolver([]),
                new FixedClock(T0.Plus(Duration.FromMinutes(8)))),
            new FixedClock(T0.Plus(Duration.FromMinutes(8))));
        await handler.HandleAsync(
            new HotelBookingRefundSucceededIntegrationEvent(
                ctx.RefundId,
                ctx.PaymentId,
                ctx.BookingId.Value,
                T0.Plus(Duration.FromMinutes(7)),
                1_000_000m,
                "IRR"),
            ct);
    }

    private async Task<SeededHotel> SeedPendingAsync(CancellationToken ct, bool withConfirmedReservation = false)
    {
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        var paymentId = Guid.CreateVersion7();
        var refundId = Guid.CreateVersion7();
        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            db.HotelBookings.Add(booking);
            db.HotelRateOfferSnapshots.Add(AcceptOffer(booking));
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
            if (withConfirmedReservation)
            {
                var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
                var attempt = reservation.StartAttempt(T0);
                reservation.ConfirmAttempt(
                    attempt.Id,
                    T0.Plus(Duration.FromMinutes(1)),
                    $"src-res-{booking.Id.Value:N}",
                    "CNF-pay",
                    booking.Rooms.Select(r => r.Id).ToArray(),
                    booking.Rooms.Select(r => r.Id).ToArray());
                db.HotelSupplierReservations.Add(reservation);
            }

            await db.SaveChangesAsync(ct);
            bookingId = booking.Id;
        }

        return new SeededHotel(bookingId, paymentId, refundId);
    }

    private static FakeHotelPaymentEvidence SuccessEvidence(SeededHotel ctx) =>
        new()
        {
            Next = new HotelBookingPaymentSuccessEvidenceRead(
                ctx.PaymentId,
                ctx.BookingId.Value,
                "Succeeded",
                1_000_000m,
                "IRR",
                true),
        };

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

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking)
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
    }

    private sealed class FakeHotelPaymentEvidence : IPaymentSuccessEvidenceQuery
    {
        public HotelBookingPaymentSuccessEvidenceRead? Next { get; set; }

        public Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PaymentSuccessEvidenceRead?>(null);

        public Task<HotelBookingPaymentSuccessEvidenceRead?> GetByHotelBookingIdAsync(
            Guid hotelBookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Next);
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }

    private sealed record SeededHotel(HotelBookingId BookingId, Guid PaymentId, Guid RefundId);
}
