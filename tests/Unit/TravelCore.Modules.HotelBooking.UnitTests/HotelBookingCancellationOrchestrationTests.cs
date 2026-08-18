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

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingCancellationOrchestrationTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);

    [Fact]
    public async Task Penalty_Zero_Cancels_Then_Enqueues_Full_Refund()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        var service = CreateService(db, source, T0);
        var result = await service.RequestAsync(ctx.Booking.Id, "cancel-1");
        Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, result.Outcome);
        Assert.Equal(1, source.CancelCalls);
        Assert.Equal(HotelBookingStatus.Cancelled, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Equal(
            HotelSupplierReservationStatus.Cancelled,
            (await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == ctx.Booking.Id)).Status);
        var cancellation = await db.HotelBookingCancellations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.HotelBookingId == ctx.Booking.Id);
        Assert.Equal(HotelBookingCancellationStatus.RefundPending, cancellation.Status);
        Assert.Equal(
            HotelSupplierCancellationAttemptStatus.Confirmed,
            cancellation.Attempts.Single().Status);
        Assert.Equal(
            1,
            await db.OutboxMessages.CountAsync(x =>
                x.MessageType == HotelBookingCancellationRefundOutboxBoundary.MessageType
                && x.Id == cancellation.Id.Value));
        var evt = HotelBookingCancellationRefundOutboxSerializer.Deserialize(
            (await db.OutboxMessages.SingleAsync(x => x.Id == cancellation.Id.Value)).Payload);
        Assert.Equal(cancellation.Id.Value, evt.HotelBookingCancellationId);
        Assert.Equal(ctx.Booking.Id.Value, evt.HotelBookingId);
        Assert.Equal(ctx.PaymentId, evt.PaymentId);
    }

    [Fact]
    public async Task Penalty_Total_Completes_Without_Refund_Outbox()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        var service = CreateService(db, source, T0.Plus(Duration.FromDays(2)));
        var result = await service.RequestAsync(ctx.Booking.Id, "cancel-full-penalty");
        Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, result.Outcome);
        Assert.Equal(1, source.CancelCalls);
        var cancellation = await db.HotelBookingCancellations.SingleAsync(x => x.HotelBookingId == ctx.Booking.Id);
        Assert.Equal(HotelBookingCancellationStatus.Completed, cancellation.Status);
        Assert.Equal(HotelBookingCancellationFinancialOutcome.NoRefund, cancellation.FinancialOutcome);
        Assert.Equal(0, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == HotelBookingCancellationRefundOutboxBoundary.MessageType));
        Assert.Equal(HotelBookingStatus.Cancelled, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
    }

    [Fact]
    public async Task Partial_Penalty_Blocks_Before_Supplier_Side_Effect()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, partialPenalty: true);
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        var service = CreateService(db, source, T0);
        var result = await service.RequestAsync(ctx.Booking.Id, "cancel-partial");
        Assert.Equal(HotelBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported, result.Outcome);
        Assert.Equal(0, source.CancelCalls);
        Assert.Equal(0, await db.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == ctx.Booking.Id));
        Assert.Equal(0, await db.HotelSupplierCancellationAttempts.CountAsync());
        Assert.Equal(0, await db.OutboxMessages.CountAsync());
        Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Equal(
            HotelSupplierReservationStatus.Confirmed,
            (await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == ctx.Booking.Id)).Status);
    }

    [Fact]
    public async Task Timeout_Leaves_Confirmed_And_Blocks_Retry()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource { CancelException = new TimeoutException("network") };
        var service = CreateService(db, source, T0);
        var result = await service.RequestAsync(ctx.Booking.Id, "cancel-timeout");
        Assert.Equal(HotelBookingCancellationRequestOutcome.Accepted, result.Outcome);
        var cancellation = await db.HotelBookingCancellations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.HotelBookingId == ctx.Booking.Id);
        Assert.Equal(HotelBookingCancellationStatus.SupplierCancellationPending, cancellation.Status);
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Initiated, cancellation.Attempts.Single().Status);
        Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Equal(0, await db.OutboxMessages.CountAsync());
        var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RetryFailedAsync(cancellation.Id, "cancel-retry"));
        Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);
        var same = await service.RequestAsync(ctx.Booking.Id, "cancel-timeout");
        Assert.Equal(cancellation.Id, same.Cancellation!.Id);
        Assert.Equal(1, source.CancelCalls);
    }

    [Fact]
    public async Task Recheck_Confirmed_Converges_And_Failed_Allows_Retry()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var timeout = new FakeHotelReservationSource { CancelException = new TimeoutException("network") };
        var service = CreateService(db, timeout, T0);
        var requested = await service.RequestAsync(ctx.Booking.Id, "cancel-recheck");
        var cancellationId = requested.Cancellation!.Id;

        var recheckSource = new FakeHotelReservationSource
        {
            NextCancelQuery = new HotelReservationCancellationQueryResult(
                HotelReservationCancellationQueryStatus.Cancelled),
        };
        var recheck = CreateService(db, recheckSource, T0.Plus(Duration.FromMinutes(1)));
        await recheck.RecheckAsync(cancellationId);
        Assert.Equal(HotelBookingStatus.Cancelled, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Equal(
            HotelBookingCancellationStatus.RefundPending,
            (await db.HotelBookingCancellations.SingleAsync(x => x.Id == cancellationId)).Status);

        await using var failedDb = CreateDb();
        var failedCtx = await SeedConfirmedAsync(failedDb);
        var failedSource = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Failed),
        };
        var failedService = CreateService(failedDb, failedSource, T0);
        var failed = await failedService.RequestAsync(failedCtx.Booking.Id, "cancel-fail");
        Assert.Equal(
            HotelSupplierCancellationAttemptStatus.Failed,
            failed.Cancellation!.Attempts.Single().Status);
        Assert.Equal(HotelBookingStatus.Confirmed, (await failedDb.HotelBookings.SingleAsync()).Status);
        var retrySource = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        var retryService = CreateService(failedDb, retrySource, T0.Plus(Duration.FromMinutes(1)));
        await retryService.RetryFailedAsync(failed.Cancellation.Id, "cancel-fail-retry");
        Assert.Equal(HotelBookingStatus.Cancelled, (await failedDb.HotelBookings.SingleAsync()).Status);
        Assert.Equal(1, retrySource.CancelCalls);
    }

    [Fact]
    public async Task Pending_Missing_Payment_And_Unconfigured_Source_Do_Not_Call_Supplier()
    {
        await using var pendingDb = CreateDb();
        var pending = await SeedPendingAsync(pendingDb);
        var source = new FakeHotelReservationSource();
        var pendingResult = await CreateService(pendingDb, source, T0).RequestAsync(pending.Id, "cancel-pending");
        Assert.Equal(HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported, pendingResult.Outcome);
        Assert.Equal(0, source.CancelCalls);

        await using var missingDb = CreateDb();
        var missing = await SeedConfirmedAsync(missingDb, withPayment: false);
        var missingResult = await CreateService(missingDb, source, T0).RequestAsync(missing.Booking.Id, "cancel-missing");
        Assert.Equal(HotelBookingCancellationRequestOutcome.MissingPaymentEvidence, missingResult.Outcome);
        Assert.Equal(0, source.CancelCalls);
        Assert.Contains(
            await missingDb.HotelBookingReconciliationIssues.Where(x => x.HotelBookingId == missing.Booking.Id).ToListAsync(),
            i => i.Kind == HotelBookingReconciliationIssueKind.MissingPaymentEvidence);
        Assert.Equal(HotelBookingStatus.Confirmed, (await missingDb.HotelBookings.SingleAsync()).Status);

        await using var noneDb = CreateDb();
        var confirmed = await SeedConfirmedAsync(noneDb);
        var none = new HotelBookingCancellationService(
            noneDb,
            new HotelReservationSourceResolver([]),
            new FixedClock(T0));
        var unconfigured = await none.RequestAsync(confirmed.Booking.Id, "cancel-none");
        Assert.Equal(HotelBookingCancellationRequestOutcome.UnconfiguredReservationSource, unconfigured.Outcome);
        Assert.Equal(0, await noneDb.HotelBookingCancellations.CountAsync());
    }

    [Fact]
    public async Task Duplicate_Key_And_Second_Key_Share_One_Process_And_Unverified_Callback_Does_Not_Mutate()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource { CancelException = new TimeoutException("network") };
        var service = CreateService(db, source, T0);
        var first = await service.RequestAsync(ctx.Booking.Id, "same-key");
        var second = await service.RequestAsync(ctx.Booking.Id, "same-key");
        var other = await service.RequestAsync(ctx.Booking.Id, "other-key");
        Assert.Equal(first.Cancellation!.Id, second.Cancellation!.Id);
        Assert.Equal(first.Cancellation.Id, other.Cancellation!.Id);
        Assert.Equal(1, await db.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == ctx.Booking.Id));
        Assert.Equal(1, source.CancelCalls);

        await service.ApplyCallbackAsync(
            first.Cancellation.Id,
            new HotelReservationCancellationQueryResult(HotelReservationCancellationQueryStatus.Cancelled),
            sourceVerified: false);
        Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Contains(
            await db.HotelBookingReconciliationIssues.Where(x => x.HotelBookingId == ctx.Booking.Id).ToListAsync(),
            i => i.Kind == HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous);
    }

    [Fact]
    public async Task Refund_Success_Completes_Cancellation_And_Does_Not_Reopen()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-refund");
        var handler = new HotelBookingRefundSucceededIntegrationHandler(
            db,
            new HotelAvailabilityHoldService(
                db,
                new TravelCore.Modules.HotelBooking.Infrastructure.Availability.HotelAvailabilitySourceResolver([]),
                new FixedClock(T0.Plus(Duration.FromMinutes(5)))),
            new FixedClock(T0.Plus(Duration.FromMinutes(5))));
        var message = new HotelBookingRefundSucceededIntegrationEvent(
            Guid.CreateVersion7(),
            ctx.PaymentId,
            ctx.Booking.Id.Value,
            T0.Plus(Duration.FromMinutes(5)),
            1_000_000m,
            "IRR");
        await handler.HandleAsync(message);
        await handler.HandleAsync(message);
        var cancellation = await db.HotelBookingCancellations.SingleAsync(x => x.HotelBookingId == ctx.Booking.Id);
        Assert.Equal(HotelBookingCancellationStatus.Completed, cancellation.Status);
        Assert.Equal(HotelBookingStatus.Cancelled, (await db.HotelBookings.SingleAsync(x => x.Id == ctx.Booking.Id)).Status);
        Assert.Equal(1, await db.RefundSuccessInbox.CountAsync());
        Assert.Equal(0, await db.RefundInvariantIssues.CountAsync());
    }

    [Fact]
    public async Task Cross_Booking_Cancellation_Does_Not_Affect_The_Other()
    {
        await using var db = CreateDb();
        var a = await SeedConfirmedAsync(db);
        var b = await SeedConfirmedAsync(db);
        var source = new FakeHotelReservationSource
        {
            NextCancel = new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Confirmed),
        };
        await CreateService(db, source, T0).RequestAsync(a.Booking.Id, "cancel-a");
        Assert.Equal(HotelBookingStatus.Cancelled, (await db.HotelBookings.SingleAsync(x => x.Id == a.Booking.Id)).Status);
        Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == b.Booking.Id)).Status);
        Assert.Equal(
            HotelSupplierReservationStatus.Confirmed,
            (await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == b.Booking.Id)).Status);
    }

    [Fact]
    public async Task R6_Compensation_Path_Does_Not_Start_R7()
    {
        await using var db = CreateDb();
        var pending = await SeedPendingAsync(db, withPayment: true);
        db.PaymentCompensationEvidence.Add(
            HotelBookingPaymentCompensationEvidence.Create(
                pending.Id,
                Guid.CreateVersion7(),
                HotelBookingPaymentCompensationReason.HoldExpired,
                T0));
        await db.SaveChangesAsync();
        var source = new FakeHotelReservationSource();
        var result = await CreateService(db, source, T0).RequestAsync(pending.Id, "cancel-r6");
        Assert.Equal(HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported, result.Outcome);
        Assert.Equal(0, source.CancelCalls);
    }

    private static HotelBookingCancellationService CreateService(
        HotelBookingDbContext db,
        IHotelReservationSource source,
        Instant now) =>
        new(db, new HotelReservationSourceResolver([source]), new FixedClock(now));

    private static HotelBookingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<HotelBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new HotelBookingDbContext(options);
    }

    private static async Task<Stay> SeedPendingAsync(HotelBookingDbContext db, bool withPayment = false)
    {
        var booking = CreateBooking();
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, partialPenalty: false));
        if (withPayment)
        {
            db.HotelBookingPaymentEvidence.Add(
                HotelBookingPaymentEvidence.Record(booking.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0));
        }

        await db.SaveChangesAsync();
        return booking;
    }

    private static async Task<ConfirmedSeed> SeedConfirmedAsync(
        HotelBookingDbContext db,
        bool withPayment = true,
        bool partialPenalty = false)
    {
        var booking = CreateBooking();
        var snapshot = AcceptOffer(booking, partialPenalty);
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
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(snapshot);
        db.HotelSupplierReservations.Add(reservation);
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
        if (withPayment)
        {
            db.HotelBookingPaymentEvidence.Add(evidence);
        }

        await db.SaveChangesAsync();
        return new ConfirmedSeed(booking, paymentId);
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

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking, bool partialPenalty)
    {
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        IReadOnlyList<HotelCancellationPenaltyRuleDraft> rules = partialPenalty
            ?
            [
                new HotelCancellationPenaltyRuleDraft(T0, null, new MoneyValue(200_000m, irr)),
            ]
            :
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, new MoneyValue(1_000_000m, irr)),
            ];
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
            rules,
            propertyTimeZoneId: "Asia/Tehran");
    }

    private sealed record ConfirmedSeed(Stay Booking, Guid PaymentId);

    private sealed class FakeHotelReservationSource : IHotelReservationSource
    {
        public ReservationSourceKey Key { get; } = new("test-source");
        public bool RequiresActiveHold { get; set; }
        public bool NotFoundProvesNoReservation { get; set; }
        public HotelReservationCancellationSourceResult? NextCancel { get; set; }
        public HotelReservationCancellationQueryResult? NextCancelQuery { get; set; }
        public Exception? CancelException { get; set; }
        public int CancelCalls { get; private set; }

        public Task<HotelReservationSourceResult> CreateReservationAsync(
            HotelReservationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("CreateReservationAsync is not used in R7 tests.");

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
            if (CancelException is not null)
            {
                throw CancelException;
            }

            return Task.FromResult(NextCancel
                ?? throw new InvalidOperationException("NextCancel is required."));
        }

        public Task<HotelReservationCancellationQueryResult> QueryCancellationStatusAsync(
            HotelReservationCancellationQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NextCancelQuery
                ?? new HotelReservationCancellationQueryResult(HotelReservationCancellationQueryStatus.PendingUnknown));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
