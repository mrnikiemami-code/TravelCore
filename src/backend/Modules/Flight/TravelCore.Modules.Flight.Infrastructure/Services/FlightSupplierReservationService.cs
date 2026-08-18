using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightSupplierReservationService
{
    public const string UnconfiguredSourceKey = "unconfigured";

    private readonly FlightDbContext _db;
    private readonly IFlightReservationSourceResolver _resolver;
    private readonly IClock _clock;

    public FlightSupplierReservationService(
        FlightDbContext db,
        IFlightReservationSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<FlightSupplierReservation> InitiateAsync(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        FlightSourceKey? requestedSourceKey = null,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingReservation = await LoadReservationForBookingAsync(flightBookingId, cancellationToken);
        if (existingReservation is not null)
        {
            var existingIdempotency = await _db.FlightSupplierReservationIdempotency
                .SingleOrDefaultAsync(
                    x => x.ReservationId == existingReservation.Id && x.IdempotencyKey == idempotencyKey.Trim(),
                    cancellationToken);
            if (existingIdempotency is not null)
            {
                return existingReservation;
            }
        }

        if (existingReservation is { HasUnresolvedAttempt: true })
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated attempt blocks another reservation attempt.");
        }

        if (existingReservation is { Status: FlightSupplierReservationStatus.Confirmed })
        {
            throw new InvalidOperationException("Confirmed reservation cannot start another attempt.");
        }

        if (existingReservation is { Status: FlightSupplierReservationStatus.Expired }
            or { Status: FlightSupplierReservationStatus.Cancelled })
        {
            throw new InvalidOperationException(
                $"{existingReservation.Status} reservation cannot start another attempt.");
        }

        var booking = await LoadBookingAsync(flightBookingId, cancellationToken);
        var snapshot = await LoadAcceptedOfferAsync(flightBookingId, cancellationToken);
        if (requestedSourceKey is { } explicitKey
            && !string.Equals(explicitKey.Value, snapshot.SourceKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Reservation source must match the accepted FlightOfferSnapshot.SourceKey.");
        }

        var sourceKey = ResolveSourceKey(requestedSourceKey, snapshot.SourceKey);

        var source = _resolver.Resolve(sourceKey);
        if (source is null || !source.Capabilities.Contains(FlightSourceCapability.ReservationCreate))
        {
            throw new InvalidOperationException(
                "Flight reservation source is unconfigured; a supplier reservation cannot be fabricated.");
        }

        FlightSupplierReservation reservation;
        if (existingReservation is null)
        {
            reservation = FlightSupplierReservation.StartPending(booking.Id, sourceKey.Value, now);
            _db.FlightSupplierReservations.Add(reservation);
        }
        else
        {
            reservation = existingReservation;
        }

        var attempt = reservation.StartAttempt(now);
        _db.FlightSupplierReservationIdempotency.Add(
            new FlightSupplierReservationIdempotencyRecord(reservation.Id, idempotencyKey, attempt.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            return await LoadReservationForBookingAsync(flightBookingId, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent reservation attempt conflict.");
        }

        reservation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        FlightReservationSourceResult result;
        try
        {
            result = await source.CreateReservationAsync(
                ToRequest(booking, snapshot, idempotencyKey),
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return reservation;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return reservation;
        }
        catch (TimeoutException)
        {
            return reservation;
        }

        ApplyCreateResult(booking, snapshot, reservation, attempt, result, now);
        await _db.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<FlightSupplierReservation> RecheckAsync(
        FlightSupplierReservationId reservationId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var reservation = await LoadReservationAsync(reservationId, cancellationToken);
        var source = _resolver.Resolve(new FlightSourceKey(reservation.SourceKey));
        if (source is null
            || !source.Capabilities.Contains(FlightSourceCapability.ReservationQuery)
            || string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            return reservation;
        }

        FlightReservationQueryResult query;
        try
        {
            query = await source.QueryReservationStatusAsync(
                reservation.SourceReservationReference,
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return reservation;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return reservation;
        }
        catch (TimeoutException)
        {
            return reservation;
        }

        var booking = await LoadBookingAsync(reservation.FlightBookingId, cancellationToken);
        var snapshot = await LoadAcceptedOfferAsync(reservation.FlightBookingId, cancellationToken);
        var openAttempt = reservation.Attempts.SingleOrDefault(a => a.IsUnresolved);

        switch (query.Status)
        {
            case FlightReservationQueryStatus.Confirmed:
                if (reservation.Status is FlightSupplierReservationStatus.Expired
                    or FlightSupplierReservationStatus.Cancelled)
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                        now,
                        reservation.Id,
                        openAttempt?.Id,
                        "Confirmed query contradicts a terminal FlightSupplierReservation.");
                    break;
                }

                if (openAttempt is null && reservation.Status == FlightSupplierReservationStatus.Confirmed)
                {
                    break;
                }

                if (openAttempt is null)
                {
                    break;
                }

                ApplyConfirmedEvidence(
                    booking,
                    snapshot,
                    reservation,
                    openAttempt,
                    query.ConfirmedSegments,
                    query.ConfirmedPassengers,
                    query.ReportedTotal,
                    query.SourceReservationReference ?? reservation.SourceReservationReference,
                    query.ReservationLocator ?? reservation.ReservationLocator,
                    query.ReservationExpiresAt,
                    query.SourceOfferReference,
                    now);
                break;
            case FlightReservationQueryStatus.Expired:
                try
                {
                    reservation.ExpireFromSource(now);
                }
                catch (InvalidOperationException)
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                        now,
                        reservation.Id,
                        openAttempt?.Id,
                        "Expired source evidence contradicts a Cancelled FlightSupplierReservation.");
                }

                break;
            case FlightReservationQueryStatus.Cancelled:
                try
                {
                    reservation.CancelFromAuthoritativeSource(now);
                    if (openAttempt is not null)
                    {
                        reservation.FailAttempt(openAttempt.Id, now);
                    }
                }
                catch (InvalidOperationException)
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                        now,
                        reservation.Id,
                        openAttempt?.Id,
                        "Cancelled source evidence contradicts an Expired FlightSupplierReservation.");
                }

                break;
            case FlightReservationQueryStatus.NotCreated:
                if (reservation.Status == FlightSupplierReservationStatus.Confirmed)
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                        now,
                        reservation.Id,
                        openAttempt?.Id,
                        "NotCreated contradicts a Confirmed FlightSupplierReservation.");
                    break;
                }

                if (openAttempt is not null && source.NotFoundProvesNoReservation)
                {
                    reservation.FailAttempt(openAttempt.Id, now);
                }
                else if (openAttempt is not null)
                {
                    PersistIssue(
                        booking.Id,
                        FlightReconciliationIssueKind.AmbiguousReservationOutcome,
                        now,
                        reservation.Id,
                        openAttempt.Id,
                        "NotCreated does not prove no reservation.");
                }

                break;
            case FlightReservationQueryStatus.PendingOrUnknown:
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    private void ApplyCreateResult(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot,
        FlightSupplierReservation reservation,
        FlightSupplierReservationAttempt attempt,
        FlightReservationSourceResult result,
        Instant now)
    {
        if (result.Outcome is FlightReservationSourceOutcome.Timeout or FlightReservationSourceOutcome.Unknown)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.AmbiguousReservationOutcome,
                now,
                reservation.Id,
                attempt.Id,
                result.Outcome.ToString());
            return;
        }

        if (result.Outcome == FlightReservationSourceOutcome.Failed)
        {
            reservation.FailAttempt(attempt.Id, now);
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.SourceReservationReference))
        {
            reservation.RecordSourceCorrelation(
                result.SourceReservationReference,
                result.ReservationLocator,
                result.ReservationExpiresAt);
        }

        if (result.Outcome == FlightReservationSourceOutcome.Partial)
        {
            var partialKinds = FlightReservationReconciliation.CollectIssues(
                ToSegments(booking),
                ToPassengers(booking),
                result.ConfirmedSegments,
                result.ConfirmedPassengers,
                snapshot.SourceOfferReference,
                result.SourceOfferReference,
                snapshot.Monetary.Total,
                result.ReportedTotal);
            if (partialKinds.Count == 0)
            {
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.ItineraryMismatch,
                    now,
                    reservation.Id,
                    attempt.Id,
                    "Partial reservation cannot confirm FlightSupplierReservation.");
                return;
            }

            foreach (var kind in partialKinds.Where(k => k is not FlightReconciliationIssueKind.AmbiguousReservationOutcome))
            {
                PersistIssue(booking.Id, kind, now, reservation.Id, attempt.Id, kind.ToString());
            }

            if (partialKinds.All(k => k == FlightReconciliationIssueKind.AmbiguousReservationOutcome))
            {
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.ItineraryMismatch,
                    now,
                    reservation.Id,
                    attempt.Id,
                    "Partial reservation cannot confirm FlightSupplierReservation.");
            }

            return;
        }

        ApplyConfirmedEvidence(
            booking,
            snapshot,
            reservation,
            attempt,
            result.ConfirmedSegments,
            result.ConfirmedPassengers,
            result.ReportedTotal,
            result.SourceReservationReference,
            result.ReservationLocator,
            result.ReservationExpiresAt,
            result.SourceOfferReference,
            now);
    }

    private void ApplyConfirmedEvidence(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot,
        FlightSupplierReservation reservation,
        FlightSupplierReservationAttempt attempt,
        IReadOnlyList<FlightOfferSegmentIdentity> confirmedSegments,
        IReadOnlyList<FlightReservationPassengerFact> confirmedPassengers,
        MoneyValue? reportedTotal,
        string? sourceReservationReference,
        string? reservationLocator,
        Instant? reservationExpiresAt,
        string? sourceOfferReference,
        Instant now)
    {
        var expectedSegments = ToSegments(booking);
        var expectedPassengers = ToPassengers(booking);
        var kinds = FlightReservationReconciliation.CollectIssues(
            expectedSegments,
            expectedPassengers,
            confirmedSegments,
            confirmedPassengers,
            snapshot.SourceOfferReference,
            sourceOfferReference,
            snapshot.Monetary.Total,
            reportedTotal);
        if (kinds.Count > 0)
        {
            foreach (var kind in kinds)
            {
                PersistIssue(booking.Id, kind, now, reservation.Id, attempt.Id, kind.ToString());
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(sourceReservationReference)
            || string.IsNullOrWhiteSpace(reservationLocator))
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.AmbiguousReservationOutcome,
                now,
                reservation.Id,
                attempt.Id,
                "Confirmed evidence lacked source reservation reference or ReservationLocator.");
            return;
        }

        reservation.ConfirmAttempt(
            attempt.Id,
            now,
            sourceReservationReference,
            reservationLocator,
            reservationExpiresAt,
            confirmedSegments,
            expectedSegments,
            confirmedPassengers,
            expectedPassengers);
    }

    private void PersistIssue(
        FlightBookingId flightBookingId,
        FlightReconciliationIssueKind kind,
        Instant now,
        FlightSupplierReservationId reservationId,
        FlightSupplierReservationAttemptId? attemptId,
        string detail) =>
        _db.FlightReconciliationIssues.Add(
            new FlightReconciliationIssue(flightBookingId, kind, now, reservationId, attemptId, detail));

    private FlightSourceKey ResolveSourceKey(FlightSourceKey? requestedSourceKey, string acceptedSourceKey)
    {
        if (requestedSourceKey is { } explicitKey)
        {
            if (_resolver.Resolve(explicitKey) is null && explicitKey.Value != UnconfiguredSourceKey)
            {
                throw new InvalidOperationException("Reservation source selection is server-controlled.");
            }

            return explicitKey;
        }

        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        return new FlightSourceKey(acceptedSourceKey);
    }

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
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        if (snapshot is null)
        {
            throw new InvalidOperationException(
                "Flight supplier reservation requires an accepted FlightOfferSnapshot.");
        }

        return snapshot;
    }

    private async Task<FlightSupplierReservation?> LoadReservationForBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        await _db.FlightSupplierReservations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);

    private async Task<FlightSupplierReservation> LoadReservationAsync(
        FlightSupplierReservationId reservationId,
        CancellationToken cancellationToken) =>
        await _db.FlightSupplierReservations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == reservationId, cancellationToken);

    private static FlightReservationRequest ToRequest(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot,
        string idempotencyKey) =>
        new(
            booking.Id.Value,
            booking.TripType,
            ToSegments(booking),
            ToPassengers(booking),
            snapshot.Id.Value,
            snapshot.SourceOfferReference,
            snapshot.Monetary.Total,
            idempotencyKey);

    private static IReadOnlyList<FlightOfferSegmentIdentity> ToSegments(FlightBookingAggregate booking) =>
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

    private static IReadOnlyList<FlightReservationPassengerFact> ToPassengers(FlightBookingAggregate booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();

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
