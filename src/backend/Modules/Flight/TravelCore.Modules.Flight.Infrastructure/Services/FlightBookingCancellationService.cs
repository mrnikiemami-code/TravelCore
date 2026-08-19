using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightBookingCancellationService
{
    private readonly FlightDbContext _db;
    private readonly IFlightCancellationSourceResolver _resolver;
    private readonly IClock _clock;

    public FlightBookingCancellationService(
        FlightDbContext db,
        IFlightCancellationSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<FlightBookingCancellationRequestResult> RequestAsync(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        var now = _clock.GetCurrentInstant();
        var existingByKey = await _db.FlightBookingCancellationIdempotency
            .SingleOrDefaultAsync(
                x => x.FlightBookingId == flightBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingByKey is not null)
        {
            var existing = await LoadCancellationAsync(existingByKey.CancellationId, cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.Accepted,
                existing);
        }

        var existingProcess = await LoadCancellationForBookingAsync(flightBookingId, cancellationToken);
        if (existingProcess is not null)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.Accepted,
                existingProcess);
        }

        var booking = await LoadBookingAsync(flightBookingId, cancellationToken);
        if (booking.Status == FlightBookingStatus.Cancelled)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.AlreadyCancelled);
        }

        if (booking.Status != FlightBookingStatus.Confirmed)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported);
        }

        var reservation = await LoadReservationForBookingAsync(flightBookingId, cancellationToken);
        if (reservation is null || reservation.Status != FlightSupplierReservationStatus.Confirmed)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported);
        }

        var snapshot = await LoadAcceptedOfferAsync(flightBookingId, cancellationToken);
        var paymentEvidence = await _db.FlightBookingPaymentEvidence
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        if (paymentEvidence is null)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.MissingPaymentEvidence);
        }

        var localEvaluation = FlightCancellationPenaltyEvaluator.Evaluate(snapshot.FareRules, snapshot.Monetary);
        if (localEvaluation.Kind == FlightCancellationPenaltyEvaluationKind.NoDeterministicRule)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: localEvaluation);
        }

        if (localEvaluation.Kind == FlightCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported,
                evaluation: localEvaluation);
        }

        var source = ResolveOwningSource(reservation);
        if (source is null
            || !source.Capabilities.Contains(FlightSourceCapability.CancellationQuote)
            || !source.Capabilities.Contains(FlightSourceCapability.ReservationCancel)
            || !source.Capabilities.Contains(FlightSourceCapability.CancellationQuery)
            || (!source.Capabilities.Contains(FlightSourceCapability.TicketVoid)
                && !source.Capabilities.Contains(FlightSourceCapability.TicketRefund)))
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource,
                evaluation: localEvaluation);
        }

        if (string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            throw new InvalidOperationException(
                "Confirmed supplier reservation is missing SourceReservationReference.");
        }

        var tickets = await LoadTicketsAsync(flightBookingId, cancellationToken);
        var issued = tickets.Where(t => t.Status == FlightTicketStatus.Issued).ToArray();
        FlightCancellationQuoteResult quote;
        try
        {
            quote = await source.QuoteCancellationAsync(
                new FlightCancellationQuoteRequest(
                    booking.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference,
                    issued.Select(t => new FlightTicketReversalIdentity(
                        t.Id.Value,
                        t.PassengerId.Value,
                        t.SourceTicketNumber
                            ?? throw new InvalidOperationException("Issued ticket is missing SourceTicketNumber.")))
                        .ToArray()),
                cancellationToken);
        }
        catch (Exception ex) when (IsTimeout(ex, cancellationToken))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Cancellation quote timeout/unknown does not start irreversible reversal.");
            await _db.SaveChangesAsync(cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: localEvaluation);
        }

        if (quote.Outcome is FlightCancellationQuoteSourceOutcome.Timeout
            or FlightCancellationQuoteSourceOutcome.Unknown)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                quote.Outcome.ToString());
            await _db.SaveChangesAsync(cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: localEvaluation);
        }

        if (quote.Outcome != FlightCancellationQuoteSourceOutcome.Complete)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                quote.Outcome.ToString());
            await _db.SaveChangesAsync(cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: localEvaluation);
        }

        var quotedEvaluation = FlightCancellationPenaltyEvaluator.EvaluatePenalty(
            quote.PenaltyAmount,
            snapshot.Monetary.Total,
            quote.PartialRefundRequired);
        if (quotedEvaluation.Kind == FlightCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported,
                evaluation: quotedEvaluation);
        }

        if (quotedEvaluation.Kind == FlightCancellationPenaltyEvaluationKind.NoDeterministicRule
            || quotedEvaluation.Kind != localEvaluation.Kind)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierEconomicsMismatch,
                now,
                reservation.Id,
                "Supplier cancellation quote disagrees with accepted fare-rule economics; snapshots were not mutated.");
            await _db.SaveChangesAsync(cancellationToken);
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.SupplierEconomicsMismatch,
                evaluation: localEvaluation);
        }

        if (quote.TicketReversalKind is not FlightSupplierReversalKind.TicketVoid
            and not FlightSupplierReversalKind.TicketRefund)
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: localEvaluation);
        }

        var ticketKind = quote.TicketReversalKind.Value;
        if (ticketKind == FlightSupplierReversalKind.TicketVoid
            && !source.Capabilities.Contains(FlightSourceCapability.TicketVoid))
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource,
                evaluation: localEvaluation);
        }

        if (ticketKind == FlightSupplierReversalKind.TicketRefund
            && !source.Capabilities.Contains(FlightSourceCapability.TicketRefund))
        {
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource,
                evaluation: localEvaluation);
        }

        var cancellation = FlightBookingCancellation.StartRequested(
            booking.Id,
            paymentEvidence.PaymentId,
            now,
            localEvaluation);
        _db.FlightBookingCancellations.Add(cancellation);
        _db.FlightBookingCancellationIdempotency.Add(
            new FlightBookingCancellationIdempotencyRecord(
                booking.Id,
                idempotencyKey,
                cancellation.Id,
                attemptId: null,
                now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            var winner = await LoadCancellationForBookingAsync(flightBookingId, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent cancellation request conflict.");
            return new FlightBookingCancellationRequestResult(
                FlightBookingCancellationRequestOutcome.Accepted,
                winner);
        }

        var ticketAttempts = new List<(FlightSupplierReversalAttempt Attempt, FlightTicket Ticket)>();
        foreach (var ticket in issued)
        {
            var attempt = cancellation.StartAttempt(ticketKind, now, ticket.Id, ticket.PassengerId);
            ticketAttempts.Add((attempt, ticket));
        }

        var reservationAttempt = cancellation.StartAttempt(FlightSupplierReversalKind.ReservationCancel, now);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var (attempt, ticket) in ticketAttempts)
        {
            await InitiateTicketReversalAsync(
                source,
                booking,
                reservation,
                cancellation,
                attempt,
                ticket,
                ticketKind,
                idempotencyKey,
                now,
                cancellationToken);
        }

        await InitiateReservationCancelAsync(
            source,
            booking,
            reservation,
            cancellation,
            reservationAttempt,
            idempotencyKey,
            now,
            cancellationToken);

        await TryCompleteAuthoritativeReversalAsync(
            booking,
            reservation,
            cancellation,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return new FlightBookingCancellationRequestResult(
            FlightBookingCancellationRequestOutcome.Accepted,
            cancellation,
            localEvaluation);
    }

    public async Task<FlightBookingCancellation> RecheckAsync(
        FlightBookingCancellationId cancellationId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var cancellation = await LoadCancellationAsync(cancellationId, cancellationToken);
        var booking = await LoadBookingAsync(cancellation.FlightBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.FlightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightSupplierReservation was not found.");

        if (cancellation.Status is FlightBookingCancellationStatus.Completed
            or FlightBookingCancellationStatus.RefundPending)
        {
            return cancellation;
        }

        var source = ResolveOwningSource(reservation);
        if (source is null || string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            return cancellation;
        }

        var tickets = await LoadTicketsAsync(booking.Id, cancellationToken);
        foreach (var attempt in cancellation.Attempts.Where(a => a.IsUnresolved).ToArray())
        {
            if (attempt.Kind == FlightSupplierReversalKind.ReservationCancel)
            {
                await QueryReservationCancelAsync(
                    source,
                    booking,
                    reservation,
                    cancellation,
                    attempt,
                    now,
                    sourceVerified: true,
                    cancellationToken);
            }
            else
            {
                var ticket = tickets.Single(t => attempt.TicketId is { } ticketId && t.Id.Equals(ticketId));
                await QueryTicketReversalAsync(
                    source,
                    booking,
                    reservation,
                    cancellation,
                    attempt,
                    ticket,
                    now,
                    sourceVerified: true,
                    cancellationToken);
            }
        }

        await TryCompleteAuthoritativeReversalAsync(
            booking,
            reservation,
            cancellation,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    public async Task<FlightBookingCancellation> RetryFailedAsync(
        FlightBookingCancellationId cancellationId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        var now = _clock.GetCurrentInstant();
        var cancellation = await LoadCancellationAsync(cancellationId, cancellationToken);
        if (cancellation.HasUnresolvedAttempt)
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated reversal attempt blocks another attempt.");
        }

        if (cancellation.Status is FlightBookingCancellationStatus.Completed
            or FlightBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                "Completed supplier reversal forbids another reversal attempt.");
        }

        if (!cancellation.Attempts.Any(a => a.Status == FlightSupplierReversalAttemptStatus.Failed))
        {
            throw new InvalidOperationException("Explicit retry is allowed only after an authoritative Failed attempt.");
        }

        var booking = await LoadBookingAsync(cancellation.FlightBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.FlightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightSupplierReservation was not found.");
        var source = ResolveOwningSource(reservation)
            ?? throw new InvalidOperationException(
                "Flight cancellation source is unconfigured; a supplier reversal cannot be fabricated.");
        if (string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            throw new InvalidOperationException(
                "Confirmed supplier reservation is missing SourceReservationReference.");
        }

        var tickets = await LoadTicketsAsync(booking.Id, cancellationToken);
        var failed = cancellation.Attempts
            .Where(a => a.Status == FlightSupplierReversalAttemptStatus.Failed)
            .GroupBy(a => (a.Kind, a.TicketId, a.PassengerId))
            .Select(g => g.Last())
            .ToArray();

        _db.FlightBookingCancellationIdempotency.Add(
            new FlightBookingCancellationIdempotencyRecord(
                booking.Id,
                idempotencyKey,
                cancellation.Id,
                attemptId: null,
                now));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            return await LoadCancellationAsync(cancellationId, cancellationToken);
        }

        foreach (var prior in failed)
        {
            if (prior.Kind == FlightSupplierReversalKind.ReservationCancel)
            {
                if (reservation.Status == FlightSupplierReservationStatus.Cancelled)
                {
                    continue;
                }

                var attempt = cancellation.StartAttempt(FlightSupplierReversalKind.ReservationCancel, now);
                await _db.SaveChangesAsync(cancellationToken);
                await InitiateReservationCancelAsync(
                    source,
                    booking,
                    reservation,
                    cancellation,
                    attempt,
                    idempotencyKey,
                    now,
                    cancellationToken);
            }
            else
            {
                var ticket = tickets.Single(t => prior.TicketId is { } ticketId && t.Id.Equals(ticketId));
                if (ticket.Status is FlightTicketStatus.Voided or FlightTicketStatus.Refunded)
                {
                    continue;
                }

                var attempt = cancellation.StartAttempt(prior.Kind, now, ticket.Id, ticket.PassengerId);
                await _db.SaveChangesAsync(cancellationToken);
                await InitiateTicketReversalAsync(
                    source,
                    booking,
                    reservation,
                    cancellation,
                    attempt,
                    ticket,
                    prior.Kind,
                    idempotencyKey,
                    now,
                    cancellationToken);
            }
        }

        await TryCompleteAuthoritativeReversalAsync(
            booking,
            reservation,
            cancellation,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    public async Task<FlightBookingCancellation> ApplyCallbackAsync(
        FlightBookingCancellationId cancellationId,
        FlightCancellationQueryResult? reservationQuery,
        IReadOnlyList<FlightTicketReversalQueryResult>? ticketQueries,
        bool sourceVerified,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var cancellation = await LoadCancellationAsync(cancellationId, cancellationToken);
        var booking = await LoadBookingAsync(cancellation.FlightBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.FlightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightSupplierReservation was not found.");

        if (!sourceVerified)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Unverified supplier cancellation callback cannot mutate state.");
            await _db.SaveChangesAsync(cancellationToken);
            return cancellation;
        }

        var tickets = await LoadTicketsAsync(booking.Id, cancellationToken);
        if (reservationQuery is not null)
        {
            var open = cancellation.Attempts.SingleOrDefault(
                a => a.Kind == FlightSupplierReversalKind.ReservationCancel && a.IsUnresolved);
            if (open is not null)
            {
                ApplyReservationQuery(booking, reservation, cancellation, open, reservationQuery, now);
            }
        }

        if (ticketQueries is not null)
        {
            foreach (var query in ticketQueries)
            {
                var open = cancellation.Attempts.SingleOrDefault(
                    a => a.TicketId is { } ticketId
                        && ticketId.Value == query.TicketId
                        && a.IsUnresolved);
                if (open is null)
                {
                    continue;
                }

                var ticket = tickets.Single(t => t.Id.Value == query.TicketId);
                ApplyTicketQuery(booking, reservation, cancellation, open, ticket, query, now);
            }
        }

        await TryCompleteAuthoritativeReversalAsync(
            booking,
            reservation,
            cancellation,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    private async Task InitiateTicketReversalAsync(
        IFlightCancellationSource source,
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightTicket ticket,
        FlightSupplierReversalKind kind,
        string idempotencyKey,
        Instant now,
        CancellationToken cancellationToken)
    {
        cancellation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        FlightTicketReversalSourceResult result;
        try
        {
            result = await source.ReverseTicketAsync(
                new FlightTicketReversalRequest(
                    booking.Id.Value,
                    cancellation.Id.Value,
                    ticket.Id.Value,
                    ticket.PassengerId.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference!,
                    ticket.SourceTicketNumber!,
                    kind,
                    $"{idempotencyKey.Trim()}:{ticket.Id.Value:D}"),
                cancellationToken);
        }
        catch (Exception ex) when (IsTimeout(ex, cancellationToken))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "NetworkTimeout does not prove ticket reversal Failed or Succeeded.");
            return;
        }

        ApplyTicketInitiateResult(booking, reservation, cancellation, attempt, ticket, result, now);
    }

    private async Task InitiateReservationCancelAsync(
        IFlightCancellationSource source,
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        string idempotencyKey,
        Instant now,
        CancellationToken cancellationToken)
    {
        cancellation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        FlightReservationCancelSourceResult result;
        try
        {
            result = await source.CancelReservationAsync(
                new FlightReservationCancelRequest(
                    booking.Id.Value,
                    cancellation.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference!,
                    idempotencyKey.Trim()),
                cancellationToken);
        }
        catch (Exception ex) when (IsTimeout(ex, cancellationToken))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "NetworkTimeout does not prove reservation cancellation Failed or Succeeded.");
            return;
        }

        ApplyReservationInitiateResult(booking, reservation, cancellation, attempt, result, now);
    }

    private async Task QueryReservationCancelAsync(
        IFlightCancellationSource source,
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        Instant now,
        bool sourceVerified,
        CancellationToken cancellationToken)
    {
        if (!sourceVerified)
        {
            return;
        }

        FlightCancellationQueryResult query;
        try
        {
            query = await source.QueryCancellationStatusAsync(
                new FlightCancellationQueryRequest(
                    booking.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference!,
                    sourceVerified: true),
                cancellationToken);
        }
        catch (Exception ex) when (IsTimeout(ex, cancellationToken))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Cancellation recheck timeout remains unresolved.");
            return;
        }

        ApplyReservationQuery(booking, reservation, cancellation, attempt, query, now);
    }

    private async Task QueryTicketReversalAsync(
        IFlightCancellationSource source,
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightTicket ticket,
        Instant now,
        bool sourceVerified,
        CancellationToken cancellationToken)
    {
        if (!sourceVerified)
        {
            return;
        }

        FlightTicketReversalQueryResult query;
        try
        {
            query = await source.QueryTicketReversalStatusAsync(
                new FlightTicketReversalQueryRequest(
                    ticket.Id.Value,
                    ticket.PassengerId.Value,
                    reservation.SourceKey,
                    ticket.SourceTicketNumber!,
                    sourceVerified: true),
                cancellationToken);
        }
        catch (Exception ex) when (IsTimeout(ex, cancellationToken))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Ticket reversal recheck timeout remains unresolved.");
            return;
        }

        ApplyTicketQuery(booking, reservation, cancellation, attempt, ticket, query, now);
    }

    private void ApplyTicketInitiateResult(
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightTicket ticket,
        FlightTicketReversalSourceResult result,
        Instant now)
    {
        if (result.Outcome is FlightTicketReversalSourceOutcome.Timeout
            or FlightTicketReversalSourceOutcome.Unknown)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                result.Outcome.ToString());
            return;
        }

        if (result.Outcome == FlightTicketReversalSourceOutcome.Failed)
        {
            cancellation.FailAttempt(attempt.Id, now);
            return;
        }

        ApplyAuthoritativeTicketReversal(
            ticket,
            result.Outcome == FlightTicketReversalSourceOutcome.Refunded,
            now);
        cancellation.SucceedAttempt(attempt.Id, now);
    }

    private void ApplyReservationInitiateResult(
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightReservationCancelSourceResult result,
        Instant now)
    {
        if (result.Outcome is FlightReservationCancelSourceOutcome.Timeout
            or FlightReservationCancelSourceOutcome.Unknown)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                result.Outcome.ToString());
            return;
        }

        if (result.Outcome == FlightReservationCancelSourceOutcome.Failed)
        {
            cancellation.FailAttempt(attempt.Id, now);
            return;
        }

        reservation.CancelFromAuthoritativeSource(now);
        cancellation.SucceedAttempt(attempt.Id, now);
    }

    private void ApplyReservationQuery(
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightCancellationQueryResult query,
        Instant now)
    {
        switch (query.Status)
        {
            case FlightCancellationQueryStatus.PendingUnknown:
            case FlightCancellationQueryStatus.NotFound:
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                    now,
                    reservation.Id,
                    query.Status.ToString());
                break;
            case FlightCancellationQueryStatus.Active:
                cancellation.FailAttempt(attempt.Id, now);
                break;
            case FlightCancellationQueryStatus.Cancelled:
                reservation.CancelFromAuthoritativeSource(now);
                cancellation.SucceedAttempt(attempt.Id, now);
                break;
        }
    }

    private void ApplyTicketQuery(
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        FlightSupplierReversalAttempt attempt,
        FlightTicket ticket,
        FlightTicketReversalQueryResult query,
        Instant now)
    {
        switch (query.Status)
        {
            case FlightTicketReversalQueryStatus.PendingUnknown:
            case FlightTicketReversalQueryStatus.NotFound:
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.SupplierCancellationAmbiguous,
                    now,
                    reservation.Id,
                    query.Status.ToString());
                break;
            case FlightTicketReversalQueryStatus.Issued:
                cancellation.FailAttempt(attempt.Id, now);
                break;
            case FlightTicketReversalQueryStatus.Voided:
                ticket.MarkVoided(now);
                cancellation.SucceedAttempt(attempt.Id, now);
                break;
            case FlightTicketReversalQueryStatus.Refunded:
                ticket.MarkRefunded(now);
                cancellation.SucceedAttempt(attempt.Id, now);
                break;
        }
    }

    private static void ApplyAuthoritativeTicketReversal(FlightTicket ticket, bool refunded, Instant now)
    {
        if (refunded)
        {
            ticket.MarkRefunded(now);
        }
        else
        {
            ticket.MarkVoided(now);
        }
    }

    private async Task TryCompleteAuthoritativeReversalAsync(
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingCancellation cancellation,
        Instant now,
        CancellationToken cancellationToken)
    {
        var tickets = await LoadTicketsAsync(booking.Id, cancellationToken);
        var required = booking.Passengers.Select(p => p.Id).ToHashSet();
        var reversed = tickets
            .Where(t => t.Status is FlightTicketStatus.Voided or FlightTicketStatus.Refunded)
            .Select(t => t.PassengerId)
            .ToHashSet();
        var issuedRemaining = tickets.Any(t => t.Status == FlightTicketStatus.Issued);
        var partialReversed = reversed.Count > 0 && !required.SetEquals(reversed);

        if (reservation.Status == FlightSupplierReservationStatus.Cancelled && issuedRemaining)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.TicketStillActive,
                now,
                reservation.Id,
                "Reservation cancelled while passenger tickets remain Issued.");
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                now,
                reservation.Id,
                "Supplier reservation Cancelled contradicts still-active tickets.");
            return;
        }

        if (partialReversed && booking.Status == FlightBookingStatus.Confirmed)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.PartialTicketReversal,
                now,
                reservation.Id,
                "Partial passenger ticket reversal cannot cancel the whole FlightBooking.");
            return;
        }

        if (reservation.Status != FlightSupplierReservationStatus.Cancelled
            || !required.SetEquals(reversed)
            || booking.Status != FlightBookingStatus.Confirmed)
        {
            return;
        }

        try
        {
            booking.CancelFromAuthoritativeSupplierReversal(reservation, tickets, now);
            cancellation.AdvanceAfterAuthoritativeCompleteReversal(now);
            FlightBookingCancellationRefundOutboxWriter.Enqueue(_db, cancellation, now);
        }
        catch (InvalidOperationException ex)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                now,
                reservation.Id,
                ex.Message);
        }
    }

    private IFlightCancellationSource? ResolveOwningSource(FlightSupplierReservation reservation)
    {
        var source = _resolver.Resolve(new FlightSourceKey(reservation.SourceKey));
        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        return source;
    }

    private void PersistIssue(
        FlightBookingId flightBookingId,
        FlightReconciliationIssueKind kind,
        Instant now,
        FlightSupplierReservationId? reservationId,
        string detail) =>
        _db.FlightReconciliationIssues.Add(
            new FlightReconciliationIssue(flightBookingId, kind, now, reservationId, attemptId: null, detail));

    private async Task<FlightBookingAggregate> LoadBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightBookings
            .Include(x => x.Journeys)
            .ThenInclude(x => x.Segments)
            .Include(x => x.Passengers)
            .SingleAsync(x => x.Id == flightBookingId, cancellationToken);

    private async Task<FlightOfferSnapshot> LoadAcceptedOfferAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _db.FlightOfferSnapshots
            .Include(x => x.Monetary)
            .ThenInclude(x => x.CategoryFares)
            .Include(x => x.FareRules)
            .ThenInclude(x => x.Baggage)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        if (snapshot is null)
        {
            throw new InvalidOperationException(
                "FlightBooking cancellation requires an accepted FlightOfferSnapshot.");
        }

        return snapshot;
    }

    private async Task<FlightSupplierReservation?> LoadReservationForBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightSupplierReservations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);

    private async Task<List<FlightTicket>> LoadTicketsAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightTickets
            .Where(x => x.FlightBookingId == flightBookingId)
            .ToListAsync(cancellationToken);

    private async Task<FlightBookingCancellation?> LoadCancellationForBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightBookingCancellations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);

    private async Task<FlightBookingCancellation> LoadCancellationAsync(
        FlightBookingCancellationId cancellationId,
        CancellationToken cancellationToken) =>
        await _db.FlightBookingCancellations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == cancellationId, cancellationToken);

    private static bool IsTimeout(Exception exception, CancellationToken cancellationToken) =>
        exception is TimeoutException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}
