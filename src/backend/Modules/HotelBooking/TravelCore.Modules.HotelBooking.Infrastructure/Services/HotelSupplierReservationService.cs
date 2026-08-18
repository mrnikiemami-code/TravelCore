using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

public sealed class HotelSupplierReservationService
{
    public const string UnconfiguredSourceKey = "unconfigured";

    private readonly HotelBookingDbContext _db;
    private readonly IHotelReservationSourceResolver _resolver;
    private readonly IClock _clock;

    public HotelSupplierReservationService(
        HotelBookingDbContext db,
        IHotelReservationSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<HotelSupplierReservation> InitiateAsync(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        ReservationSourceKey? requestedSourceKey = null,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingReservation = await LoadReservationForBookingAsync(hotelBookingId, cancellationToken);
        if (existingReservation is not null)
        {
            var existingIdempotency = await _db.HotelSupplierReservationIdempotency
                .SingleOrDefaultAsync(
                    x => x.ReservationId == existingReservation.Id && x.IdempotencyKey == idempotencyKey.Trim(),
                    cancellationToken);
            if (existingIdempotency is not null)
            {
                return existingReservation;
            }
        }

        var booking = await LoadBookingAsync(hotelBookingId, cancellationToken);
        if (booking.Status == HotelBookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled HotelBooking cannot start a supplier reservation.");
        }

        if (existingReservation is not null
            && existingReservation.Status == HotelSupplierReservationStatus.Confirmed
            && booking.Status == HotelBookingStatus.Confirmed)
        {
            return existingReservation;
        }

        if (existingReservation is { HasUnresolvedAttempt: true })
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated attempt blocks another reservation attempt.");
        }

        if (existingReservation is { Status: HotelSupplierReservationStatus.Confirmed })
        {
            throw new InvalidOperationException("Confirmed reservation cannot start another attempt.");
        }

        var snapshot = await LoadAcceptedOfferAsync(hotelBookingId, cancellationToken);
        var sourceKey = ResolveSourceKey(requestedSourceKey);
        var source = _resolver.Resolve(sourceKey);
        if (source is null)
        {
            throw new InvalidOperationException(
                "Hotel reservation source is unconfigured; a supplier reservation cannot be fabricated.");
        }

        if (source.RequiresActiveHold
            && !await HasActiveUnexpiredHoldAsync(booking, now, cancellationToken))
        {
            throw new InvalidOperationException(
                "Reservation initiation requires an Active unexpired HotelAvailabilityHold.");
        }

        HotelSupplierReservation reservation;
        HotelSupplierReservationAttempt attempt;
        if (existingReservation is null)
        {
            reservation = HotelSupplierReservation.StartPending(booking.Id, sourceKey.Value, now);
            _db.HotelSupplierReservations.Add(reservation);
        }
        else
        {
            reservation = existingReservation;
        }

        attempt = reservation.StartAttempt(now);
        _db.HotelSupplierReservationIdempotency.Add(
            new HotelSupplierReservationIdempotencyRecord(reservation.Id, idempotencyKey, attempt.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            return await LoadReservationForBookingAsync(hotelBookingId, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent reservation attempt conflict.");
        }

        reservation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        var holdReference = await LoadHoldReferenceAsync(booking.Id, now, cancellationToken);
        HotelReservationSourceResult result;
        try
        {
            result = await source.CreateReservationAsync(
                ToRequest(booking, snapshot, idempotencyKey, holdReference),
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return reservation;
        }
        catch (TimeoutException)
        {
            return reservation;
        }

        await ApplyCreateResultAsync(booking, reservation, attempt, snapshot, result, now, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<HotelSupplierReservation> RecheckAsync(
        HotelSupplierReservationId reservationId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var reservation = await LoadReservationAsync(reservationId, cancellationToken);
        if (reservation.Status == HotelSupplierReservationStatus.Confirmed)
        {
            return reservation;
        }

        var source = _resolver.Resolve(new ReservationSourceKey(reservation.SourceKey));
        if (source is null || string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            return reservation;
        }

        var query = await source.QueryReservationStatusAsync(
            reservation.SourceReservationReference,
            cancellationToken);
        var booking = await LoadBookingAsync(reservation.HotelBookingId, cancellationToken);
        var snapshot = await LoadAcceptedOfferAsync(reservation.HotelBookingId, cancellationToken);
        var openAttempt = reservation.Attempts.SingleOrDefault(a => a.IsUnresolved);

        switch (query.Status)
        {
            case HotelReservationQueryStatus.Confirmed:
                if (openAttempt is null)
                {
                    break;
                }

                await ApplyConfirmedEvidenceAsync(
                    booking,
                    reservation,
                    openAttempt,
                    snapshot,
                    query.ConfirmedRoomReservationIds,
                    query.ReportedTotal,
                    cancellationTermsMatch: null,
                    query.SourceReservationReference ?? reservation.SourceReservationReference,
                    supplierConfirmationCode: null,
                    now,
                    cancellationToken);
                break;
            case HotelReservationQueryStatus.NotCreated:
                if (openAttempt is not null)
                {
                    reservation.FailAttempt(openAttempt.Id, now);
                }

                break;
            case HotelReservationQueryStatus.NotFound:
                if (openAttempt is not null && source.NotFoundProvesNoReservation)
                {
                    reservation.FailAttempt(openAttempt.Id, now);
                }
                else if (openAttempt is not null)
                {
                    PersistIssue(
                        booking.Id,
                        HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome,
                        now,
                        reservation.Id,
                        openAttempt.Id,
                        "NotFound does not prove no reservation.");
                }

                break;
            case HotelReservationQueryStatus.Cancelled:
                try
                {
                    reservation.CancelFromSource(now);
                    if (openAttempt is not null)
                    {
                        reservation.FailAttempt(openAttempt.Id, now);
                    }
                }
                catch (InvalidOperationException)
                {
                    PersistIssue(
                        booking.Id,
                        HotelBookingReconciliationIssueKind.ContradictorySupplierEvidence,
                        now,
                        reservation.Id,
                        openAttempt?.Id,
                        "Cancelled source evidence contradicts a Confirmed HotelSupplierReservation.");
                }

                break;
            case HotelReservationQueryStatus.PendingUnknown:
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    private async Task ApplyCreateResultAsync(
        Stay booking,
        HotelSupplierReservation reservation,
        HotelSupplierReservationAttempt attempt,
        HotelRateOfferSnapshot snapshot,
        HotelReservationSourceResult result,
        Instant now,
        CancellationToken cancellationToken)
    {
        if (result.Outcome is HotelReservationSourceOutcome.Timeout or HotelReservationSourceOutcome.Unknown)
        {
            reservation.MarkAttemptInitiated(attempt.Id, now);
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome,
                now,
                reservation.Id,
                attempt.Id,
                result.Outcome.ToString());
            return;
        }

        if (result.Outcome == HotelReservationSourceOutcome.Failed)
        {
            reservation.FailAttempt(attempt.Id, now);
            return;
        }

        if (result.Outcome is HotelReservationSourceOutcome.Partial or HotelReservationSourceOutcome.Complete)
        {
            if (!string.IsNullOrWhiteSpace(result.SourceReservationReference))
            {
                reservation.RecordSourceCorrelation(
                    result.SourceReservationReference,
                    result.SupplierConfirmationCode);
            }

            if (result.Outcome == HotelReservationSourceOutcome.Complete)
            {
                await ApplyConfirmedEvidenceAsync(
                    booking,
                    reservation,
                    attempt,
                    snapshot,
                    result.ConfirmedRoomReservationIds,
                    result.ReportedTotal,
                    result.CancellationTermsMatch,
                    result.SourceReservationReference,
                    result.SupplierConfirmationCode,
                    now,
                    cancellationToken);
                return;
            }

            reservation.MarkAttemptInitiated(attempt.Id, now);
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.RoomSetMismatch,
                now,
                reservation.Id,
                attempt.Id,
                "Partial room confirmation cannot confirm HotelSupplierReservation.");
        }
    }

    private async Task ApplyConfirmedEvidenceAsync(
        Stay booking,
        HotelSupplierReservation reservation,
        HotelSupplierReservationAttempt attempt,
        HotelRateOfferSnapshot snapshot,
        IReadOnlyList<Guid> confirmedRoomReservationIds,
        TravelCore.Money.Money? reportedTotal,
        bool? cancellationTermsMatch,
        string? sourceReservationReference,
        string? supplierConfirmationCode,
        Instant now,
        CancellationToken cancellationToken)
    {
        var confirmedRooms = confirmedRoomReservationIds
            .Select(RoomReservationId.From)
            .ToArray();
        var requestedRooms = booking.Rooms.Select(r => r.Id).ToArray();
        var existingIssues = await _db.HotelBookingReconciliationIssues
            .Where(x => x.HotelBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        var kinds = booking.CollectConfirmationIssues(
            reservation,
            snapshot.Place,
            snapshot.CheckInDate,
            snapshot.CheckOutDate,
            confirmedRooms,
            reportedTotal,
            cancellationTermsMatch,
            snapshot.Monetary);
        if (kinds.Count > 0)
        {
            if (attempt.Status == HotelSupplierReservationAttemptStatus.Created)
            {
                reservation.MarkAttemptInitiated(attempt.Id, now);
            }

            foreach (var kind in kinds)
            {
                PersistIssue(booking.Id, kind, now, reservation.Id, attempt.Id, kind.ToString());
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(sourceReservationReference))
        {
            reservation.MarkAttemptInitiated(attempt.Id, now);
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome,
                now,
                reservation.Id,
                attempt.Id,
                "Confirmed evidence lacked source reservation reference.");
            return;
        }

        reservation.ConfirmAttempt(
            attempt.Id,
            now,
            sourceReservationReference,
            supplierConfirmationCode,
            confirmedRooms,
            requestedRooms);
        booking.ConfirmFromAuthoritativeSupplierReservation(
            reservation,
            now,
            snapshot.Place,
            snapshot.CheckInDate,
            snapshot.CheckOutDate,
            confirmedRooms,
            reportedTotal,
            cancellationTermsMatch,
            snapshot.Monetary,
            existingIssues);
    }

    private void PersistIssue(
        HotelBookingId hotelBookingId,
        HotelBookingReconciliationIssueKind kind,
        Instant now,
        HotelSupplierReservationId reservationId,
        HotelSupplierReservationAttemptId? attemptId,
        string detail) =>
        _db.HotelBookingReconciliationIssues.Add(
            new HotelBookingReconciliationIssue(hotelBookingId, kind, now, reservationId, attemptId, detail));

    private ReservationSourceKey ResolveSourceKey(ReservationSourceKey? requestedSourceKey)
    {
        var sourceKey = requestedSourceKey ?? new ReservationSourceKey(UnconfiguredSourceKey);
        if (requestedSourceKey is { } explicitKey && _resolver.Resolve(explicitKey) is null
            && explicitKey.Value != UnconfiguredSourceKey)
        {
            throw new InvalidOperationException("Reservation source selection is server-controlled.");
        }

        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count == 1)
        {
            return configured[0];
        }

        if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        return sourceKey;
    }

    private async Task<bool> HasActiveUnexpiredHoldAsync(
        Stay booking,
        Instant now,
        CancellationToken cancellationToken)
    {
        var holds = await _db.HotelAvailabilityHolds
            .Include(x => x.Rooms)
            .Where(x => x.HotelBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        return holds.Any(hold =>
            hold.IsActiveAndUnexpired(now)
            && hold.Rooms.Select(r => r.RoomReservationId).ToHashSet()
                .SetEquals(booking.Rooms.Select(r => r.Id)));
    }

    private async Task<Stay> LoadBookingAsync(
        HotelBookingId hotelBookingId,
        CancellationToken cancellationToken) =>
        await _db.HotelBookings
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Guests)
            .SingleAsync(x => x.Id == hotelBookingId, cancellationToken);

    private async Task<HotelRateOfferSnapshot> LoadAcceptedOfferAsync(
        HotelBookingId hotelBookingId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _db.HotelRateOfferSnapshots
            .Include(x => x.Rooms)
            .Include(x => x.Monetary)
            .ThenInclude(x => x.Charges)
            .Include(x => x.CancellationPolicy)
            .ThenInclude(x => x.Rules)
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);
        if (snapshot is null)
        {
            throw new InvalidOperationException(
                "HotelBooking confirmation prerequisites require an accepted HotelRateOfferSnapshot.");
        }

        return snapshot;
    }

    private async Task<HotelSupplierReservation?> LoadReservationForBookingAsync(
        HotelBookingId hotelBookingId,
        CancellationToken cancellationToken) =>
        await _db.HotelSupplierReservations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);

    private async Task<HotelSupplierReservation> LoadReservationAsync(
        HotelSupplierReservationId reservationId,
        CancellationToken cancellationToken) =>
        await _db.HotelSupplierReservations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == reservationId, cancellationToken);

    private async Task<string?> LoadHoldReferenceAsync(
        HotelBookingId hotelBookingId,
        Instant now,
        CancellationToken cancellationToken)
    {
        var holds = await _db.HotelAvailabilityHolds
            .Where(x => x.HotelBookingId == hotelBookingId)
            .ToListAsync(cancellationToken);
        return holds.FirstOrDefault(h => h.IsActiveAndUnexpired(now))?.SourceHoldReference;
    }

    private static HotelReservationRequest ToRequest(
        Stay booking,
        HotelRateOfferSnapshot snapshot,
        string idempotencyKey,
        string? holdReference) =>
        new(
            booking.Id.Value,
            booking.Place.PlaceId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(room => new HotelReservationRoomRequest(
                room.Id.Value,
                room.AdultCount,
                room.Guests
                    .Where(g => g.Category == HotelGuestCategory.Child)
                    .Select(g => g.AgeAtCheckIn!.Value.Years)
                    .ToArray(),
                room.Guests.Select(g => new HotelReservationGuestFact(
                    g.GivenName,
                    g.FamilyName,
                    g.IsLeadGuest,
                    (int)g.Category))
                .ToArray()))
            .ToArray(),
            snapshot.Id.Value,
            snapshot.Monetary.Total,
            holdReference,
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
