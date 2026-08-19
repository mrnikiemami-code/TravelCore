using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Cancellations;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightBookingCancellationTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly Instant ReservationExpiry = Instant.FromUtc(2026, 8, 20, 12, 0);

    [Fact]
    public void Evaluator_Maps_Zero_Total_Partial_And_Missing()
    {
        var booking = OneWayBooking();
        var full = Accept(booking, Irr(0m));
        Assert.Equal(
            FlightCancellationPenaltyEvaluationKind.FullRefund,
            FlightCancellationPenaltyEvaluator.Evaluate(full.FareRules, full.Monetary).Kind);

        var none = Accept(OneWayBooking(), Irr(1_000_000m));
        Assert.Equal(
            FlightCancellationPenaltyEvaluationKind.NoRefund,
            FlightCancellationPenaltyEvaluator.Evaluate(none.FareRules, none.Monetary).Kind);

        var partial = Accept(OneWayBooking(), Irr(100_000m));
        Assert.Equal(
            FlightCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported,
            FlightCancellationPenaltyEvaluator.Evaluate(partial.FareRules, partial.Monetary).Kind);

        var flagged = Accept(OneWayBooking(), Irr(0m), partialRefundRequired: true);
        Assert.Equal(
            FlightCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported,
            FlightCancellationPenaltyEvaluator.Evaluate(flagged.FareRules, flagged.Monetary).Kind);

        var missing = Accept(OneWayBooking(), cancelPenalty: null);
        Assert.Equal(
            FlightCancellationPenaltyEvaluationKind.NoDeterministicRule,
            FlightCancellationPenaltyEvaluator.Evaluate(missing.FareRules, missing.Monetary).Kind);
    }

    [Fact]
    public void Ticket_Void_And_Refund_Are_Issued_Only_And_Not_Payment()
    {
        var booking = OneWayBooking();
        var ticket = FlightTicket.StartPending(booking.Id, booking.Passengers[0].Id, "test-source", T0);
        Assert.Throws<InvalidOperationException>(() => ticket.MarkVoided(T0.Plus(Duration.FromMinutes(1))));
        ticket.MarkIssued("125-001", T0.Plus(Duration.FromMinutes(1)));
        ticket.MarkVoided(T0.Plus(Duration.FromMinutes(2)));
        Assert.Equal(FlightTicketStatus.Voided, ticket.Status);
        Assert.Throws<InvalidOperationException>(() => ticket.MarkRefunded(T0.Plus(Duration.FromMinutes(3))));

        var refunded = FlightTicket.StartPending(booking.Id, booking.Passengers[0].Id, "test-source", T0);
        refunded.MarkIssued("125-002", T0.Plus(Duration.FromMinutes(1)));
        refunded.MarkRefunded(T0.Plus(Duration.FromMinutes(2)));
        Assert.Equal(FlightTicketStatus.Refunded, refunded.Status);
        Assert.Equal(
            new[] { "Pending", "Issued", "Voided", "Refunded" },
            Enum.GetNames<FlightTicketStatus>());
        Assert.Null(typeof(FlightBookingAggregate).GetMethod("Cancel"));
        Assert.Null(typeof(FlightBookingAggregate).GetMethod("SetCancelled"));
        Assert.Null(typeof(FlightBookingAggregate).GetMethod("ForceCancel"));
        Assert.NotNull(typeof(FlightBookingAggregate).GetMethod(
            nameof(FlightBookingAggregate.CancelFromAuthoritativeSupplierReversal)));
        Assert.NotNull(typeof(FlightBookingAggregate).GetMethod(
            nameof(FlightBookingAggregate.CancelFromAuthoritativePaymentCompensation)));
    }

    [Fact]
    public async Task Penalty_Zero_Cancels_Then_Enqueues_Full_Refund()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        var source = SucceedingSource(Irr(0m));
        var result = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-1");
        Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, result.Outcome);
        Assert.Equal(1, source.QuoteCalls);
        Assert.Equal(1, source.ReverseTicketCalls);
        Assert.Equal(1, source.CancelReservationCalls);
        Assert.Equal(FlightBookingStatus.Cancelled, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Equal(
            FlightSupplierReservationStatus.Cancelled,
            (await db.FlightSupplierReservations.SingleAsync()).Status);
        Assert.All(await db.FlightTickets.ToListAsync(), t => Assert.Equal(FlightTicketStatus.Voided, t.Status));
        var cancellation = await db.FlightBookingCancellations.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(FlightBookingCancellationStatus.RefundPending, cancellation.Status);
        Assert.Equal(FlightBookingCancellationFinancialOutcome.FullRefund, cancellation.FinancialOutcome);
        Assert.Equal(
            1,
            await db.OutboxMessages.CountAsync(x =>
                x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType
                && x.Id == cancellation.Id.Value));
        var evt = FlightBookingCancellationRefundOutboxSerializer.Deserialize(
            (await db.OutboxMessages.SingleAsync(x => x.Id == cancellation.Id.Value)).Payload);
        Assert.Equal(cancellation.Id.Value, evt.FlightBookingCancellationId);
        Assert.Equal(ctx.Booking.Id.Value, evt.FlightBookingId);
        Assert.Equal(ctx.PaymentId, evt.PaymentId);
    }

    [Fact]
    public async Task Penalty_Total_Completes_Without_Payment_Refund()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(1_000_000m));
        var source = SucceedingSource(Irr(1_000_000m));
        var result = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-none");
        Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, result.Outcome);
        var cancellation = await db.FlightBookingCancellations.SingleAsync();
        Assert.Equal(FlightBookingCancellationStatus.Completed, cancellation.Status);
        Assert.Equal(FlightBookingCancellationFinancialOutcome.NoRefund, cancellation.FinancialOutcome);
        Assert.Equal(0, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType));
        Assert.Equal(FlightBookingStatus.Cancelled, (await db.FlightBookings.SingleAsync()).Status);
    }

    [Fact]
    public async Task Partial_Penalty_Blocks_With_Zero_Supplier_Calls()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(100_000m));
        var source = SucceedingSource(Irr(0m));
        var result = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-partial");
        Assert.Equal(FlightBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported, result.Outcome);
        Assert.Equal(0, source.QuoteCalls);
        Assert.Equal(0, source.CancelReservationCalls);
        Assert.Equal(0, source.ReverseTicketCalls);
        Assert.Equal(0, await db.FlightBookingCancellations.CountAsync());
        Assert.Equal(0, await db.OutboxMessages.CountAsync());
        Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync()).Status);
    }

    [Fact]
    public async Task Pending_Already_Cancelled_Missing_Payment_And_Unconfigured_Do_Not_Call_Source()
    {
        await using var pendingDb = CreateDb();
        var pending = OneWayBooking();
        pendingDb.FlightBookings.Add(pending);
        await pendingDb.SaveChangesAsync();
        var source = SucceedingSource(Irr(0m));
        var pendingResult = await CreateService(pendingDb, source, T0).RequestAsync(pending.Id, "cancel-pending");
        Assert.Equal(
            FlightBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported,
            pendingResult.Outcome);
        Assert.Equal(0, source.QuoteCalls);

        await using var cancelledDb = CreateDb();
        var cancelled = OneWayBooking();
        cancelled.CancelFromAuthoritativePaymentCompensation(T0.Plus(Duration.FromMinutes(1)));
        cancelledDb.FlightBookings.Add(cancelled);
        await cancelledDb.SaveChangesAsync();
        var already = await CreateService(cancelledDb, source, T0).RequestAsync(cancelled.Id, "cancel-again");
        Assert.Equal(FlightBookingCancellationRequestOutcome.AlreadyCancelled, already.Outcome);
        Assert.Equal(0, source.QuoteCalls);

        await using var missingDb = CreateDb();
        var missing = await SeedConfirmedAsync(missingDb, Irr(0m), withPayment: false);
        var missingResult = await CreateService(missingDb, source, T0).RequestAsync(missing.Booking.Id, "cancel-missing");
        Assert.Equal(FlightBookingCancellationRequestOutcome.MissingPaymentEvidence, missingResult.Outcome);
        Assert.Equal(0, source.QuoteCalls);
        Assert.Equal(FlightBookingStatus.Confirmed, (await missingDb.FlightBookings.SingleAsync()).Status);

        await using var noneDb = CreateDb();
        var confirmed = await SeedConfirmedAsync(noneDb, Irr(0m));
        var none = new FlightBookingCancellationService(
            noneDb,
            new FlightCancellationSourceResolver([]),
            new FixedClock(T0));
        var unconfigured = await none.RequestAsync(confirmed.Booking.Id, "cancel-none");
        Assert.Equal(FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource, unconfigured.Outcome);
        Assert.Equal(0, await noneDb.FlightBookingCancellations.CountAsync());
    }

    [Fact]
    public async Task Ticket_Timeout_Stays_Initiated_And_Blocks_Retry()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        var source = SucceedingSource(Irr(0m));
        source.ReverseException = new TimeoutException("network");
        var result = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-timeout");
        Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, result.Outcome);
        var cancellation = await db.FlightBookingCancellations.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(FlightBookingCancellationStatus.SupplierReversalPending, cancellation.Status);
        Assert.Contains(
            cancellation.Attempts,
            a => a.Kind is FlightSupplierReversalKind.TicketVoid or FlightSupplierReversalKind.TicketRefund
                && a.Status == FlightSupplierReversalAttemptStatus.Initiated);
        Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Equal(0, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType));
        var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(db, source, T0).RetryFailedAsync(cancellation.Id, "cancel-retry"));
        Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);
        var same = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-timeout");
        Assert.Equal(cancellation.Id, same.Cancellation!.Id);
        Assert.Equal(1, source.QuoteCalls);
        Assert.Equal(1, source.ReverseTicketCalls);
    }

    [Fact]
    public async Task Failed_Attempt_Allows_Retry_And_Recheck_Converges()
    {
        await using var failedDb = CreateDb();
        var failedCtx = await SeedConfirmedAsync(failedDb, Irr(0m));
        var failedSource = SucceedingSource(Irr(0m));
        failedSource.NextReverse = new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Failed);
        var failed = await CreateService(failedDb, failedSource, T0)
            .RequestAsync(failedCtx.Booking.Id, "cancel-fail");
        Assert.Contains(
            failed.Cancellation!.Attempts,
            a => a.Kind == FlightSupplierReversalKind.TicketVoid
                && a.Status == FlightSupplierReversalAttemptStatus.Failed);
        Assert.Equal(FlightBookingStatus.Confirmed, (await failedDb.FlightBookings.SingleAsync()).Status);

        var retrySource = SucceedingSource(Irr(0m));
        await CreateService(failedDb, retrySource, T0.Plus(Duration.FromMinutes(1)))
            .RetryFailedAsync(failed.Cancellation.Id, "cancel-fail-retry");
        Assert.Equal(FlightBookingStatus.Cancelled, (await failedDb.FlightBookings.SingleAsync()).Status);
        Assert.Equal(1, retrySource.ReverseTicketCalls);

        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        var timeout = SucceedingSource(Irr(0m));
        timeout.ReverseException = new TimeoutException("network");
        var requested = await CreateService(db, timeout, T0).RequestAsync(ctx.Booking.Id, "cancel-recheck");
        var recheckSource = SucceedingSource(Irr(0m));
        recheckSource.NextTicketQuery = new FlightTicketReversalQueryResult(
            (await db.FlightTickets.SingleAsync()).Id.Value,
            FlightTicketReversalQueryStatus.Voided);
        recheckSource.NextCancelQuery = new FlightCancellationQueryResult(FlightCancellationQueryStatus.Cancelled);
        await CreateService(db, recheckSource, T0.Plus(Duration.FromMinutes(1)))
            .RecheckAsync(requested.Cancellation!.Id);
        Assert.Equal(FlightBookingStatus.Cancelled, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Equal(
            FlightBookingCancellationStatus.RefundPending,
            (await db.FlightBookingCancellations.SingleAsync()).Status);
    }

    [Fact]
    public async Task Complete_Multi_Passenger_Reversal_Cancels_Booking()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m), twoPassengers: true);
        var source = SucceedingSource(Irr(0m));
        await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-multi");
        Assert.Equal(2, source.ReverseTicketCalls);
        Assert.Equal(1, source.CancelReservationCalls);
        Assert.Equal(FlightBookingStatus.Cancelled, (await db.FlightBookings.SingleAsync()).Status);
        Assert.All(await db.FlightTickets.ToListAsync(), t => Assert.Equal(FlightTicketStatus.Voided, t.Status));
    }

    [Fact]
    public async Task Partial_Passenger_Reversal_Leaves_Confirmed_Without_Refund()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m), twoPassengers: true);
        var source = SucceedingSource(Irr(0m));
        source.ReverseQueue.Enqueue(new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Voided));
        source.ReverseQueue.Enqueue(new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Timeout));
        source.NextCancel = new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Timeout);
        var result = await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-partial-pax");
        Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, result.Outcome);
        Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Equal(0, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType));
        Assert.Contains(
            await db.FlightReconciliationIssues.ToListAsync(),
            i => i.Kind == FlightReconciliationIssueKind.PartialTicketReversal);
        var tickets = await db.FlightTickets.ToListAsync();
        Assert.Contains(tickets, t => t.Status == FlightTicketStatus.Voided);
        Assert.Contains(tickets, t => t.Status == FlightTicketStatus.Issued);
    }

    [Fact]
    public async Task Reservation_Cancelled_With_Active_Tickets_Is_Contradiction()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        var source = SucceedingSource(Irr(0m));
        source.NextReverse = new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Timeout);
        source.NextCancel = new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Succeeded);
        await CreateService(db, source, T0).RequestAsync(ctx.Booking.Id, "cancel-contradiction");
        Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync()).Status);
        var kinds = await db.FlightReconciliationIssues
            .Where(x => x.FlightBookingId == ctx.Booking.Id)
            .Select(x => x.Kind)
            .ToListAsync();
        Assert.Contains(FlightReconciliationIssueKind.TicketStillActive, kinds);
        Assert.Contains(FlightReconciliationIssueKind.ContradictorySupplierEvidence, kinds);
        Assert.Equal(0, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType));
    }

    [Fact]
    public async Task Quote_Economics_Mismatch_And_Quote_Timeout_Do_Not_Reverse()
    {
        await using var mismatchDb = CreateDb();
        var mismatchCtx = await SeedConfirmedAsync(mismatchDb, Irr(0m));
        var mismatchSource = SucceedingSource(Irr(1_000_000m));
        var mismatch = await CreateService(mismatchDb, mismatchSource, T0)
            .RequestAsync(mismatchCtx.Booking.Id, "cancel-mismatch");
        Assert.Equal(FlightBookingCancellationRequestOutcome.SupplierEconomicsMismatch, mismatch.Outcome);
        Assert.Equal(1, mismatchSource.QuoteCalls);
        Assert.Equal(0, mismatchSource.ReverseTicketCalls);
        Assert.Equal(0, mismatchSource.CancelReservationCalls);
        Assert.Equal(0, await mismatchDb.FlightBookingCancellations.CountAsync());
        Assert.Equal(FlightBookingStatus.Confirmed, (await mismatchDb.FlightBookings.SingleAsync()).Status);

        await using var timeoutDb = CreateDb();
        var timeoutCtx = await SeedConfirmedAsync(timeoutDb, Irr(0m));
        var timeoutSource = SucceedingSource(Irr(0m));
        timeoutSource.QuoteException = new TimeoutException("quote");
        var timedOut = await CreateService(timeoutDb, timeoutSource, T0)
            .RequestAsync(timeoutCtx.Booking.Id, "cancel-quote-timeout");
        Assert.Equal(FlightBookingCancellationRequestOutcome.PolicyAmbiguous, timedOut.Outcome);
        Assert.Equal(1, timeoutSource.QuoteCalls);
        Assert.Equal(0, timeoutSource.ReverseTicketCalls);
        Assert.Equal(0, await timeoutDb.FlightBookingCancellations.CountAsync());
        Assert.Contains(
            await timeoutDb.FlightReconciliationIssues.ToListAsync(),
            i => i.Kind == FlightReconciliationIssueKind.SupplierCancellationAmbiguous);
    }

    [Fact]
    public async Task Duplicate_Key_Shares_One_Process_And_Unverified_Callback_Does_Not_Mutate()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        var source = SucceedingSource(Irr(0m));
        source.ReverseException = new TimeoutException("network");
        var service = CreateService(db, source, T0);
        var first = await service.RequestAsync(ctx.Booking.Id, "same-key");
        var second = await service.RequestAsync(ctx.Booking.Id, "same-key");
        var other = await service.RequestAsync(ctx.Booking.Id, "other-key");
        Assert.Equal(first.Cancellation!.Id, second.Cancellation!.Id);
        Assert.Equal(first.Cancellation.Id, other.Cancellation!.Id);
        Assert.Equal(1, await db.FlightBookingCancellations.CountAsync());
        Assert.Equal(1, source.QuoteCalls);

        await service.ApplyCallbackAsync(
            first.Cancellation.Id,
            new FlightCancellationQueryResult(FlightCancellationQueryStatus.Cancelled),
            [
                new FlightTicketReversalQueryResult(
                    (await db.FlightTickets.SingleAsync()).Id.Value,
                    FlightTicketReversalQueryStatus.Voided),
            ],
            sourceVerified: false);
        Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Contains(
            await db.FlightReconciliationIssues.ToListAsync(),
            i => i.Kind == FlightReconciliationIssueKind.SupplierCancellationAmbiguous);
    }

    [Fact]
    public async Task Cross_Booking_Cancellation_Is_Isolated()
    {
        await using var db = CreateDb();
        var first = await SeedConfirmedAsync(db, Irr(0m));
        var second = await SeedConfirmedAsync(db, Irr(0m));
        var source = SucceedingSource(Irr(0m));
        await CreateService(db, source, T0).RequestAsync(first.Booking.Id, "cancel-a");
        Assert.Equal(
            FlightBookingStatus.Cancelled,
            (await db.FlightBookings.SingleAsync(x => x.Id == first.Booking.Id)).Status);
        Assert.Equal(
            FlightBookingStatus.Confirmed,
            (await db.FlightBookings.SingleAsync(x => x.Id == second.Booking.Id)).Status);
        Assert.Equal(1, await db.FlightBookingCancellations.CountAsync(x => x.FlightBookingId == first.Booking.Id));
        Assert.Equal(0, await db.FlightBookingCancellations.CountAsync(x => x.FlightBookingId == second.Booking.Id));
    }

    [Fact]
    public async Task Refund_Success_Completes_Full_Refund_Cancellation()
    {
        await using var db = CreateDb();
        var ctx = await SeedConfirmedAsync(db, Irr(0m));
        await CreateService(db, SucceedingSource(Irr(0m)), T0).RequestAsync(ctx.Booking.Id, "cancel-refund");
        var handler = new FlightBookingRefundSucceededIntegrationHandler(
            db,
            new FixedClock(T0.Plus(Duration.FromMinutes(5))));
        await handler.HandleAsync(new FlightBookingRefundSucceededIntegrationEvent(
            Guid.CreateVersion7(),
            ctx.PaymentId,
            ctx.Booking.Id.Value,
            T0.Plus(Duration.FromMinutes(5)),
            1_000_000m,
            "IRR"));
        Assert.Equal(
            FlightBookingCancellationStatus.Completed,
            (await db.FlightBookingCancellations.SingleAsync()).Status);
        Assert.Equal(FlightBookingStatus.Cancelled, (await db.FlightBookings.SingleAsync()).Status);
    }

    private static FakeFlightCancellationSource SucceedingSource(MoneyValue quotePenalty) =>
        new()
        {
            NextQuote = new FlightCancellationQuoteResult(
                FlightCancellationQuoteSourceOutcome.Complete,
                quotePenalty,
                partialRefundRequired: false,
                FlightSupplierReversalKind.TicketVoid),
            NextReverse = new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Voided),
            NextCancel = new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Succeeded),
        };

    private static FlightBookingCancellationService CreateService(
        FlightDbContext db,
        IFlightCancellationSource source,
        Instant now) =>
        new(db, new FlightCancellationSourceResolver([source]), new FixedClock(now));

    private static async Task<ConfirmedSeed> SeedConfirmedAsync(
        FlightDbContext db,
        MoneyValue? cancelPenalty,
        bool twoPassengers = false,
        bool withPayment = true)
    {
        var booking = twoPassengers ? TwoPassengerBooking() : OneWayBooking();
        var snapshot = Accept(booking, cancelPenalty);
        var reservation = ConfirmedReservation(booking);
        var tickets = IssuedTickets(booking);
        var paymentId = Guid.CreateVersion7();
        var payment = FlightBookingPaymentEvidence.Record(
            booking.Id,
            paymentId,
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.Total.Currency.Value,
            T0.Plus(Duration.FromMinutes(2)));
        booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
            reservation,
            payment,
            tickets,
            snapshot.Monetary,
            [],
            T0.Plus(Duration.FromMinutes(5)));

        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(snapshot);
        db.FlightSupplierReservations.Add(reservation);
        db.FlightTickets.AddRange(tickets);
        if (withPayment)
        {
            db.FlightBookingPaymentEvidence.Add(payment);
        }

        await db.SaveChangesAsync();
        return new ConfirmedSeed(booking, paymentId);
    }

    private static FlightBookingAggregate OneWayBooking() =>
        FlightBookingAggregate.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightBookingAggregate TwoPassengerBooking() =>
        FlightBookingAggregate.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [
                new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult),
                new FlightPassengerSpecification("Alan", "Turing", FlightPassengerCategory.Child),
            ]);

    private static FlightOfferSnapshot Accept(
        FlightBookingAggregate booking,
        MoneyValue? cancelPenalty,
        bool partialRefundRequired = false) =>
        FlightOfferSnapshot.Accept(
            booking,
            T0,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            T0.Minus(Duration.FromMinutes(1)),
            Expires,
            Irr(800_000m),
            Irr(150_000m),
            Irr(50_000m),
            Irr(1_000_000m),
            Identities(booking),
            new FlightPassengerCount(
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant)),
            new FlightFareRulesDraft(
                true,
                true,
                Ticketing,
                cancelPenalty,
                Irr(80_000m),
                partialRefundRequired));

    private static FlightSupplierReservation ConfirmedReservation(FlightBookingAggregate booking)
    {
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            $"src-res-{booking.Id.Value:N}",
            "ABC123",
            ReservationExpiry,
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));
        return reservation;
    }

    private static IReadOnlyList<FlightTicket> IssuedTickets(FlightBookingAggregate booking) =>
        booking.Passengers
            .Select((p, i) =>
            {
                var ticket = FlightTicket.StartPending(booking.Id, p.Id, "test-source", T0);
                ticket.MarkIssued($"125-{booking.Id.Value:N}-{i + 1:000}", T0.Plus(Duration.FromMinutes(3)));
                return ticket;
            })
            .ToArray();

    private static FlightJourneySpecification Direct(string origin, string destination, Instant dep, Instant arr) =>
        new(
        [
            new FlightSegmentSpecification(
                new AirportReference(origin),
                new AirportReference(destination),
                dep,
                "Asia/Tehran",
                arr,
                "Europe/London",
                new AirlineReference("TK"),
                null,
                "TK800"),
        ]);

    private static IReadOnlyList<FlightOfferSegmentIdentity> Identities(FlightBookingAggregate booking) =>
        booking.Journeys
            .OrderBy(j => j.Ordinal)
            .SelectMany(j => j.Segments
                .OrderBy(s => s.Ordinal)
                .Select(s => new FlightOfferSegmentIdentity(
                    j.Ordinal,
                    s.Ordinal,
                    s.Origin,
                    s.Destination,
                    s.DepartureAt,
                    s.ArrivalAt,
                    s.MarketingCarrier,
                    s.OperatingCarrier,
                    s.FlightNumber)))
            .ToArray();

    private static IReadOnlyList<FlightReservationPassengerFact> Passengers(FlightBookingAggregate booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private static FlightDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new FlightDbContext(options);
    }

    private sealed record ConfirmedSeed(FlightBookingAggregate Booking, Guid PaymentId);

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }

    private sealed class FakeFlightCancellationSource : IFlightCancellationSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability>
            {
                FlightSourceCapability.CancellationQuote,
                FlightSourceCapability.ReservationCancel,
                FlightSourceCapability.TicketVoid,
                FlightSourceCapability.TicketRefund,
                FlightSourceCapability.CancellationQuery,
            };

        public int QuoteCalls { get; private set; }

        public int CancelReservationCalls { get; private set; }

        public int ReverseTicketCalls { get; private set; }

        public FlightCancellationQuoteResult? NextQuote { get; set; }

        public FlightReservationCancelSourceResult? NextCancel { get; set; }

        public FlightTicketReversalSourceResult? NextReverse { get; set; }

        public Queue<FlightTicketReversalSourceResult> ReverseQueue { get; } = new();

        public FlightCancellationQueryResult? NextCancelQuery { get; set; }

        public FlightTicketReversalQueryResult? NextTicketQuery { get; set; }

        public Exception? QuoteException { get; set; }

        public Exception? ReverseException { get; set; }

        public Task<FlightCancellationQuoteResult> QuoteCancellationAsync(
            FlightCancellationQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            QuoteCalls++;
            if (QuoteException is not null)
            {
                throw QuoteException;
            }

            return Task.FromResult(NextQuote
                ?? new FlightCancellationQuoteResult(FlightCancellationQuoteSourceOutcome.Unknown));
        }

        public Task<FlightReservationCancelSourceResult> CancelReservationAsync(
            FlightReservationCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            CancelReservationCalls++;
            return Task.FromResult(NextCancel
                ?? new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Unknown));
        }

        public Task<FlightTicketReversalSourceResult> ReverseTicketAsync(
            FlightTicketReversalRequest request,
            CancellationToken cancellationToken = default)
        {
            ReverseTicketCalls++;
            if (ReverseException is not null)
            {
                throw ReverseException;
            }

            if (ReverseQueue.Count > 0)
            {
                return Task.FromResult(ReverseQueue.Dequeue());
            }

            return Task.FromResult(NextReverse
                ?? new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Unknown));
        }

        public Task<FlightCancellationQueryResult> QueryCancellationStatusAsync(
            FlightCancellationQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NextCancelQuery
                ?? new FlightCancellationQueryResult(FlightCancellationQueryStatus.PendingUnknown));

        public Task<FlightTicketReversalQueryResult> QueryTicketReversalStatusAsync(
            FlightTicketReversalQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NextTicketQuery
                ?? new FlightTicketReversalQueryResult(request.TicketId, FlightTicketReversalQueryStatus.PendingUnknown));
    }
}
