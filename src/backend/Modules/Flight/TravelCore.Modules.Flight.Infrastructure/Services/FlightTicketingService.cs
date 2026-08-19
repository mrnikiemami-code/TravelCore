using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightTicketingService
{
    private readonly FlightDbContext _db;
    private readonly IFlightTicketingSourceResolver _resolver;
    private readonly IClock _clock;

    public FlightTicketingService(
        FlightDbContext db,
        IFlightTicketingSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task InitiateAsync(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingIdempotency = await _db.FlightTicketingIdempotency
            .SingleOrDefaultAsync(
                x => x.FlightBookingId == flightBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingIdempotency is not null)
        {
            return;
        }

        if (await _db.FlightTicketingAttempts.AnyAsync(
                x => x.FlightBookingId == flightBookingId &&
                     (x.Status == FlightTicketingAttemptStatus.Created
                      || x.Status == FlightTicketingAttemptStatus.Initiated),
                cancellationToken))
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated ticketing attempt blocks another issuance.");
        }

        var booking = await LoadBookingAsync(flightBookingId, cancellationToken);
        var snapshot = await LoadAcceptedOfferAsync(flightBookingId, cancellationToken);
        var reservation = await _db.FlightSupplierReservations
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("Ticketing requires a FlightSupplierReservation.");
        var paymentEvidence = await _db.FlightBookingPaymentEvidence
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("Ticketing requires Payment success evidence.");

        if (!paymentEvidence.MatchesMonetarySnapshot(snapshot.Monetary))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.PaymentEvidenceMismatch,
                now,
                "Payment evidence does not match FlightBookingMonetarySnapshot.");
            return;
        }

        if (reservation.Status == FlightSupplierReservationStatus.Expired)
        {
            await FlightBookingPaymentRecovery.RecordCompensationAsync(
                _db,
                booking.Id,
                paymentEvidence.PaymentId,
                FlightBookingPaymentCompensationReason.ReservationExpired,
                now,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (reservation.Status == FlightSupplierReservationStatus.Cancelled)
        {
            await FlightBookingPaymentRecovery.RecordCompensationAsync(
                _db,
                booking.Id,
                paymentEvidence.PaymentId,
                FlightBookingPaymentCompensationReason.ReservationCancelled,
                now,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (reservation.Status != FlightSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Ticketing requires a Confirmed FlightSupplierReservation.");
        }

        if ((reservation.ReservationExpiresAt is { } expires && expires <= now)
            || (snapshot.FareRules.TicketingDeadline is { } deadline && deadline <= now))
        {
            var reason = snapshot.FareRules.TicketingDeadline is { } ticketingDeadline && ticketingDeadline <= now
                ? FlightBookingPaymentCompensationReason.TicketingDeadlineExpired
                : FlightBookingPaymentCompensationReason.ReservationExpired;
            await FlightBookingPaymentRecovery.RecordCompensationAsync(
                _db,
                booking.Id,
                paymentEvidence.PaymentId,
                reason,
                now,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var sourceKey = new FlightSourceKey(reservation.SourceKey);
        var source = _resolver.Resolve(sourceKey);
        if (source is null
            || !string.Equals(source.Key.Value, reservation.SourceKey, StringComparison.Ordinal)
            || !source.Capabilities.Contains(FlightSourceCapability.TicketCreate))
        {
            throw new InvalidOperationException(
                "Flight ticketing source is unconfigured or does not match the reservation source.");
        }

        EnsurePendingTickets(booking, reservation.SourceKey, now);

        var attempt = FlightTicketingAttempt.StartCreated(booking.Id, now);
        _db.FlightTicketingAttempts.Add(attempt);
        _db.FlightTicketingIdempotency.Add(
            new FlightTicketingIdempotencyRecord(booking.Id, idempotencyKey, attempt.Id, now));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            return;
        }

        attempt.MarkInitiated(now);
        await _db.SaveChangesAsync(cancellationToken);

        FlightTicketingSourceResult result;
        try
        {
            result = await source.CreateTicketsAsync(
                ToRequest(booking, snapshot, reservation, idempotencyKey),
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (TimeoutException)
        {
            return;
        }

        await ApplyCreateResultAsync(booking, snapshot, reservation, paymentEvidence, attempt, result, now, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecheckAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var booking = await LoadBookingAsync(flightBookingId, cancellationToken);
        var snapshot = await LoadAcceptedOfferAsync(flightBookingId, cancellationToken);
        var reservation = await _db.FlightSupplierReservations
            .SingleAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        var paymentEvidence = await _db.FlightBookingPaymentEvidence
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        var attempt = await _db.FlightTicketingAttempts
            .SingleOrDefaultAsync(
                x => x.FlightBookingId == flightBookingId &&
                     (x.Status == FlightTicketingAttemptStatus.Created
                      || x.Status == FlightTicketingAttemptStatus.Initiated),
                cancellationToken);
        if (attempt is null || paymentEvidence is null)
        {
            return;
        }

        var source = _resolver.Resolve(new FlightSourceKey(reservation.SourceKey));
        if (source is null
            || !source.Capabilities.Contains(FlightSourceCapability.TicketQuery)
            || string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            return;
        }

        FlightTicketingQueryResult query;
        try
        {
            query = await source.QueryTicketStatusAsync(
                reservation.SourceReservationReference,
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (TimeoutException)
        {
            return;
        }

        switch (query.Status)
        {
            case FlightTicketingSourceStatus.Complete:
                ApplyIssuedTickets(booking, reservation.SourceKey, query.Tickets, now);
                if (AllPassengersIssued(booking))
                {
                    attempt.MarkSucceeded(now);
                    FlightBookingConfirmation.TryConfirm(_db, booking, reservation, paymentEvidence, snapshot, now);
                }
                else
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.TicketSetMismatch,
                        now,
                        "Ticketing query did not cover every passenger.");
                }

                break;
            case FlightTicketingSourceStatus.Partial:
                ApplyIssuedTickets(booking, reservation.SourceKey, query.Tickets, now);
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.TicketSetMismatch,
                    now,
                    "Partial ticketing cannot confirm FlightBooking.");
                break;
            case FlightTicketingSourceStatus.Failed:
            case FlightTicketingSourceStatus.NotCreated:
                if (query.NotFoundProvesNoTicket || source.NotFoundProvesNoTicket)
                {
                    attempt.MarkFailed(now);
                    if (snapshot.FareRules.TicketingDeadline is { } deadline && deadline <= now)
                    {
                        await FlightBookingPaymentRecovery.RecordCompensationAsync(
                            _db,
                            booking.Id,
                            paymentEvidence.PaymentId,
                            FlightBookingPaymentCompensationReason.TicketingDeadlineExpired,
                            now,
                            cancellationToken);
                    }
                    else
                    {
                        await FlightBookingPaymentRecovery.RecordCompensationAsync(
                            _db,
                            booking.Id,
                            paymentEvidence.PaymentId,
                            FlightBookingPaymentCompensationReason.TicketingDefinitivelyFailed,
                            now,
                            cancellationToken);
                    }
                }

                break;
            default:
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyCreateResultAsync(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot,
        FlightSupplierReservation reservation,
        FlightBookingPaymentEvidence paymentEvidence,
        FlightTicketingAttempt attempt,
        FlightTicketingSourceResult result,
        Instant now,
        CancellationToken cancellationToken)
    {
        if (result.Status is FlightTicketingSourceStatus.Timeout or FlightTicketingSourceStatus.Unknown)
        {
            return;
        }

        if (result.Status is FlightTicketingSourceStatus.Failed or FlightTicketingSourceStatus.NotCreated)
        {
            attempt.MarkFailed(now);
            await FlightBookingPaymentRecovery.RecordCompensationAsync(
                _db,
                booking.Id,
                paymentEvidence.PaymentId,
                FlightBookingPaymentCompensationReason.TicketingDefinitivelyFailed,
                now,
                cancellationToken);
            return;
        }

        ApplyIssuedTickets(booking, reservation.SourceKey, result.Tickets, now);
        if (result.Status == FlightTicketingSourceStatus.Complete && AllPassengersIssued(booking))
        {
            attempt.MarkSucceeded(now);
            FlightBookingConfirmation.TryConfirm(_db, booking, reservation, paymentEvidence, snapshot, now);
            return;
        }

        PersistIssue(
            booking.Id,
            FlightReconciliationIssueKind.TicketSetMismatch,
            now,
            "Partial or unmatched ticketing cannot confirm FlightBooking.");
    }

    private void EnsurePendingTickets(FlightBookingAggregate booking, string sourceKey, Instant now)
    {
        var existing = _db.FlightTickets.Local
            .Where(t => t.FlightBookingId.Equals(booking.Id))
            .ToList();
        if (existing.Count == 0)
        {
            existing = _db.FlightTickets.Where(t => t.FlightBookingId == booking.Id).ToList();
        }

        foreach (var passenger in booking.Passengers)
        {
            if (existing.Any(t => t.PassengerId.Equals(passenger.Id)))
            {
                continue;
            }

            _db.FlightTickets.Add(FlightTicket.StartPending(booking.Id, passenger.Id, sourceKey, now));
        }
    }

    private void ApplyIssuedTickets(
        FlightBookingAggregate booking,
        string sourceKey,
        IReadOnlyList<FlightIssuedTicketFact> facts,
        Instant now)
    {
        EnsurePendingTickets(booking, sourceKey, now);
        var tickets = _db.FlightTickets.Local
            .Where(t => t.FlightBookingId.Equals(booking.Id))
            .ToList();
        if (tickets.Count == 0)
        {
            tickets = _db.FlightTickets.Where(t => t.FlightBookingId == booking.Id).ToList();
        }

        foreach (var fact in facts)
        {
            var passenger = booking.Passengers.FirstOrDefault(p =>
                string.Equals(p.GivenName, fact.GivenName, StringComparison.Ordinal)
                && string.Equals(p.FamilyName, fact.FamilyName, StringComparison.Ordinal));
            if (passenger is null)
            {
                continue;
            }

            var ticket = tickets.Single(t => t.PassengerId.Equals(passenger.Id));
            ticket.MarkIssued(fact.SourceTicketNumber, now);
        }
    }

    private bool AllPassengersIssued(FlightBookingAggregate booking)
    {
        var tickets = _db.FlightTickets.Local
            .Where(t => t.FlightBookingId.Equals(booking.Id))
            .ToList();
        if (tickets.Count == 0)
        {
            tickets = _db.FlightTickets.Where(t => t.FlightBookingId == booking.Id).ToList();
        }

        return booking.Passengers.All(p =>
            tickets.Any(t => t.PassengerId.Equals(p.Id) && t.Status == FlightTicketStatus.Issued));
    }

    private void PersistIssue(
        FlightBookingId flightBookingId,
        FlightReconciliationIssueKind kind,
        Instant now,
        string detail) =>
        _db.FlightReconciliationIssues.Add(new FlightReconciliationIssue(flightBookingId, kind, now, detail: detail));

    private async Task<FlightBookingAggregate> LoadBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightBookings
            .Include(x => x.Passengers)
            .SingleAsync(x => x.Id == flightBookingId, cancellationToken);

    private async Task<FlightOfferSnapshot> LoadAcceptedOfferAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _db.FlightOfferSnapshots
            .Include(x => x.Monetary)
            .Include(x => x.FareRules)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        if (snapshot is null)
        {
            throw new InvalidOperationException("Ticketing requires an accepted FlightOfferSnapshot.");
        }

        return snapshot;
    }

    private static FlightTicketingRequest ToRequest(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot,
        FlightSupplierReservation reservation,
        string idempotencyKey) =>
        new(
            booking.Id.Value,
            reservation.SourceKey,
            reservation.SourceReservationReference
                ?? throw new InvalidOperationException("Confirmed reservation is missing source reference."),
            reservation.ReservationLocator
                ?? throw new InvalidOperationException("Confirmed reservation is missing ReservationLocator."),
            booking.Passengers
                .OrderBy(p => p.Ordinal)
                .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
                .ToArray(),
            snapshot.Id.Value,
            snapshot.Monetary.Total,
            idempotencyKey);

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
