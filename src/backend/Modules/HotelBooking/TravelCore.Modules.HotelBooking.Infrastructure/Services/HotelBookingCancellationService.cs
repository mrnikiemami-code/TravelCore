using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

public sealed class HotelBookingCancellationService
{
    public const string UnconfiguredSourceKey = HotelSupplierReservationService.UnconfiguredSourceKey;

    private readonly HotelBookingDbContext _db;
    private readonly IHotelReservationSourceResolver _resolver;
    private readonly IClock _clock;

    public HotelBookingCancellationService(
        HotelBookingDbContext db,
        IHotelReservationSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<HotelBookingCancellationRequestResult> RequestAsync(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        var now = _clock.GetCurrentInstant();
        var existingByKey = await _db.HotelBookingCancellationIdempotency
            .SingleOrDefaultAsync(
                x => x.HotelBookingId == hotelBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingByKey is not null)
        {
            var existing = await LoadCancellationAsync(existingByKey.CancellationId, cancellationToken);
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.Accepted,
                existing);
        }

        var existingProcess = await LoadCancellationForBookingAsync(hotelBookingId, cancellationToken);
        if (existingProcess is not null)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.Accepted,
                existingProcess);
        }

        var booking = await LoadBookingAsync(hotelBookingId, cancellationToken);
        if (booking.Status == HotelBookingStatus.Cancelled)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.AlreadyCancelled);
        }

        if (booking.Status != HotelBookingStatus.Confirmed)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported);
        }

        var reservation = await LoadReservationForBookingAsync(hotelBookingId, cancellationToken);
        if (reservation is null || reservation.Status != HotelSupplierReservationStatus.Confirmed)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported);
        }

        if (await _db.PaymentCompensationEvidence.AnyAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken))
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported);
        }

        var snapshot = await LoadAcceptedOfferAsync(hotelBookingId, cancellationToken);
        var paymentEvidence = await _db.HotelBookingPaymentEvidence
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);
        if (paymentEvidence is null)
        {
            PersistIssue(
                hotelBookingId,
                HotelBookingReconciliationIssueKind.MissingPaymentEvidence,
                now,
                reservation.Id,
                "Confirmed HotelBooking lacks authoritative Payment evidence; supplier cancellation was not started.");
            await _db.SaveChangesAsync(cancellationToken);
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.MissingPaymentEvidence);
        }

        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            now);
        if (evaluation.Kind == HotelCancellationPenaltyEvaluationKind.NoDeterministicRule)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.PolicyAmbiguous,
                evaluation: evaluation);
        }

        if (evaluation.Kind == HotelCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported,
                evaluation: evaluation);
        }

        var source = ResolveOwningSource(reservation);
        if (source is null)
        {
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.UnconfiguredReservationSource,
                evaluation: evaluation);
        }

        if (string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            throw new InvalidOperationException(
                "Confirmed supplier reservation is missing SourceReservationReference.");
        }

        var cancellation = HotelBookingCancellation.StartRequested(
            booking.Id,
            paymentEvidence.PaymentId,
            now,
            evaluation);
        var attempt = cancellation.StartAttempt(now);
        _db.HotelBookingCancellations.Add(cancellation);
        _db.HotelBookingCancellationIdempotency.Add(
            new HotelBookingCancellationIdempotencyRecord(
                booking.Id,
                idempotencyKey,
                cancellation.Id,
                attempt.Id,
                now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            var winner = await LoadCancellationForBookingAsync(hotelBookingId, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent cancellation request conflict.");
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.Accepted,
                winner);
        }

        cancellation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        HotelReservationCancellationSourceResult result;
        try
        {
            result = await source.InitiateCancellationAsync(
                new HotelReservationCancellationRequest(
                    booking.Id.Value,
                    cancellation.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference,
                    idempotencyKey.Trim()),
                cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "NetworkTimeout does not prove supplier cancellation Failed or Confirmed.");
            await _db.SaveChangesAsync(cancellationToken);
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.Accepted,
                cancellation,
                evaluation);
        }
        catch (TimeoutException)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "NetworkTimeout does not prove supplier cancellation Failed or Confirmed.");
            await _db.SaveChangesAsync(cancellationToken);
            return new HotelBookingCancellationRequestResult(
                HotelBookingCancellationRequestOutcome.Accepted,
                cancellation,
                evaluation);
        }

        await ApplyInitiateResultAsync(
            booking,
            reservation,
            cancellation,
            attempt,
            evaluation,
            result,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return new HotelBookingCancellationRequestResult(
            HotelBookingCancellationRequestOutcome.Accepted,
            cancellation,
            evaluation);
    }

    public async Task<HotelBookingCancellation> RecheckAsync(
        HotelBookingCancellationId cancellationId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var cancellation = await LoadCancellationAsync(cancellationId, cancellationToken);
        var booking = await LoadBookingAsync(cancellation.HotelBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.HotelBookingId, cancellationToken)
            ?? throw new InvalidOperationException("HotelSupplierReservation was not found.");

        if (cancellation.Status is HotelBookingCancellationStatus.Completed
            or HotelBookingCancellationStatus.RefundPending)
        {
            return cancellation;
        }

        var openAttempt = cancellation.Attempts.SingleOrDefault(a => a.IsUnresolved);
        if (openAttempt is null)
        {
            return cancellation;
        }

        var source = ResolveOwningSource(reservation);
        if (source is null || string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            return cancellation;
        }

        HotelReservationCancellationQueryResult query;
        try
        {
            query = await source.QueryCancellationStatusAsync(
                new HotelReservationCancellationQueryRequest(
                    booking.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference,
                    sourceVerified: true),
                cancellationToken);
        }
        catch (TimeoutException)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Cancellation recheck timeout remains unresolved.");
            await _db.SaveChangesAsync(cancellationToken);
            return cancellation;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Cancellation recheck timeout remains unresolved.");
            await _db.SaveChangesAsync(cancellationToken);
            return cancellation;
        }

        await ApplyQueryResultAsync(
            booking,
            reservation,
            cancellation,
            openAttempt,
            query,
            now,
            sourceVerified: true,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    public async Task<HotelBookingCancellation> RetryFailedAsync(
        HotelBookingCancellationId cancellationId,
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
                "An unresolved Created/Initiated cancellation attempt blocks another attempt.");
        }

        if (cancellation.Attempts.Any(a => a.Status == HotelSupplierCancellationAttemptStatus.Confirmed)
            || cancellation.Status is HotelBookingCancellationStatus.Completed
                or HotelBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                "Confirmed supplier cancellation forbids another cancellation attempt.");
        }

        if (!cancellation.Attempts.Any(a => a.Status == HotelSupplierCancellationAttemptStatus.Failed))
        {
            throw new InvalidOperationException("Explicit retry is allowed only after an authoritative Failed attempt.");
        }

        var booking = await LoadBookingAsync(cancellation.HotelBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.HotelBookingId, cancellationToken)
            ?? throw new InvalidOperationException("HotelSupplierReservation was not found.");
        var source = ResolveOwningSource(reservation)
            ?? throw new InvalidOperationException(
                "Hotel reservation source is unconfigured; a supplier cancellation cannot be fabricated.");
        if (string.IsNullOrWhiteSpace(reservation.SourceReservationReference))
        {
            throw new InvalidOperationException(
                "Confirmed supplier reservation is missing SourceReservationReference.");
        }

        var snapshot = await LoadAcceptedOfferAsync(booking.Id, cancellationToken);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            cancellation.RequestedAt);

        var attempt = cancellation.StartAttempt(now);
        _db.HotelBookingCancellationIdempotency.Add(
            new HotelBookingCancellationIdempotencyRecord(
                booking.Id,
                idempotencyKey,
                cancellation.Id,
                attempt.Id,
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

        cancellation.MarkAttemptInitiated(attempt.Id, now);
        await _db.SaveChangesAsync(cancellationToken);

        HotelReservationCancellationSourceResult result;
        try
        {
            result = await source.InitiateCancellationAsync(
                new HotelReservationCancellationRequest(
                    booking.Id.Value,
                    cancellation.Id.Value,
                    reservation.SourceKey,
                    reservation.SourceReservationReference,
                    idempotencyKey.Trim()),
                cancellationToken);
        }
        catch (Exception ex) when (ex is TimeoutException
            || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "NetworkTimeout does not prove supplier cancellation Failed or Confirmed.");
            await _db.SaveChangesAsync(cancellationToken);
            return cancellation;
        }

        await ApplyInitiateResultAsync(
            booking,
            reservation,
            cancellation,
            attempt,
            evaluation,
            result,
            now,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    public async Task<HotelBookingCancellation> ApplyCallbackAsync(
        HotelBookingCancellationId cancellationId,
        HotelReservationCancellationQueryResult query,
        bool sourceVerified,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var now = _clock.GetCurrentInstant();
        var cancellation = await LoadCancellationAsync(cancellationId, cancellationToken);
        var booking = await LoadBookingAsync(cancellation.HotelBookingId, cancellationToken);
        var reservation = await LoadReservationForBookingAsync(cancellation.HotelBookingId, cancellationToken)
            ?? throw new InvalidOperationException("HotelSupplierReservation was not found.");

        if (!sourceVerified)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                "Unverified supplier cancellation callback cannot mutate state.");
            await _db.SaveChangesAsync(cancellationToken);
            return cancellation;
        }

        var openAttempt = cancellation.Attempts.SingleOrDefault(a => a.IsUnresolved);
        if (openAttempt is null
            && cancellation.Status is HotelBookingCancellationStatus.Completed
                or HotelBookingCancellationStatus.RefundPending)
        {
            return cancellation;
        }

        if (openAttempt is null)
        {
            return cancellation;
        }

        await ApplyQueryResultAsync(
            booking,
            reservation,
            cancellation,
            openAttempt,
            query,
            now,
            sourceVerified: true,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return cancellation;
    }

    private async Task ApplyInitiateResultAsync(
        Stay booking,
        HotelSupplierReservation reservation,
        HotelBookingCancellation cancellation,
        HotelSupplierCancellationAttempt attempt,
        HotelCancellationPenaltyEvaluation evaluation,
        HotelReservationCancellationSourceResult result,
        Instant now,
        CancellationToken cancellationToken)
    {
        if (result.Outcome is HotelReservationCancellationSourceOutcome.Timeout
            or HotelReservationCancellationSourceOutcome.Unknown)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                now,
                reservation.Id,
                result.Outcome.ToString());
            return;
        }

        if (result.Outcome == HotelReservationCancellationSourceOutcome.Failed)
        {
            cancellation.FailAttempt(attempt.Id, now);
            return;
        }

        RecordEconomicsMismatchIfNeeded(booking.Id, reservation.Id, evaluation, result.ReportedCancellationFee, now);
        ApplyAuthoritativeSupplierCancellation(booking, reservation, cancellation, attempt, now);
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task ApplyQueryResultAsync(
        Stay booking,
        HotelSupplierReservation reservation,
        HotelBookingCancellation cancellation,
        HotelSupplierCancellationAttempt attempt,
        HotelReservationCancellationQueryResult query,
        Instant now,
        bool sourceVerified,
        CancellationToken cancellationToken)
    {
        if (!sourceVerified)
        {
            return;
        }

        var snapshot = await LoadAcceptedOfferAsync(booking.Id, cancellationToken);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            cancellation.RequestedAt);

        switch (query.Status)
        {
            case HotelReservationCancellationQueryStatus.PendingUnknown:
            case HotelReservationCancellationQueryStatus.NotFound:
                PersistIssue(
                    booking.Id,
                    HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous,
                    now,
                    reservation.Id,
                    query.Status.ToString());
                break;
            case HotelReservationCancellationQueryStatus.Active:
                cancellation.FailAttempt(attempt.Id, now);
                break;
            case HotelReservationCancellationQueryStatus.Cancelled:
                RecordEconomicsMismatchIfNeeded(
                    booking.Id,
                    reservation.Id,
                    evaluation,
                    query.ReportedCancellationFee,
                    now);
                ApplyAuthoritativeSupplierCancellation(booking, reservation, cancellation, attempt, now);
                break;
        }
    }

    private void ApplyAuthoritativeSupplierCancellation(
        Stay booking,
        HotelSupplierReservation reservation,
        HotelBookingCancellation cancellation,
        HotelSupplierCancellationAttempt attempt,
        Instant now)
    {
        try
        {
            reservation.CancelFromAuthoritativeSupplierCancellation(now);
            booking.CancelFromAuthoritativeSupplierCancellation(reservation, now);
            cancellation.ConfirmAttempt(attempt.Id, now);
            HotelBookingCancellationRefundOutboxWriter.Enqueue(_db, cancellation, now);
        }
        catch (InvalidOperationException ex)
        {
            PersistIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.SupplierCancellationContradiction,
                now,
                reservation.Id,
                ex.Message);
        }
    }

    private void RecordEconomicsMismatchIfNeeded(
        HotelBookingId hotelBookingId,
        HotelSupplierReservationId reservationId,
        HotelCancellationPenaltyEvaluation evaluation,
        MoneyValue? reportedFee,
        Instant now)
    {
        if (reportedFee is null || evaluation.Penalty is null)
        {
            return;
        }

        if (reportedFee.Currency != evaluation.Penalty.Currency
            || reportedFee.Amount != evaluation.Penalty.Amount)
        {
            PersistIssue(
                hotelBookingId,
                HotelBookingReconciliationIssueKind.SupplierCancellationEconomicsMismatch,
                now,
                reservationId,
                "Supplier cancellation economics differ from the immutable policy; customer outcome is unchanged.");
        }
    }

    private IHotelReservationSource? ResolveOwningSource(HotelSupplierReservation reservation)
    {
        var source = _resolver.Resolve(new ReservationSourceKey(reservation.SourceKey));
        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        return source;
    }

    private void PersistIssue(
        HotelBookingId hotelBookingId,
        HotelBookingReconciliationIssueKind kind,
        Instant now,
        HotelSupplierReservationId? reservationId,
        string detail) =>
        _db.HotelBookingReconciliationIssues.Add(
            new HotelBookingReconciliationIssue(hotelBookingId, kind, now, reservationId, attemptId: null, detail));

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
                "HotelBooking cancellation requires an accepted HotelRateOfferSnapshot.");
        }

        return snapshot;
    }

    private async Task<HotelSupplierReservation?> LoadReservationForBookingAsync(
        HotelBookingId hotelBookingId,
        CancellationToken cancellationToken) =>
        await _db.HotelSupplierReservations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);

    private async Task<HotelBookingCancellation?> LoadCancellationForBookingAsync(
        HotelBookingId hotelBookingId,
        CancellationToken cancellationToken) =>
        await _db.HotelBookingCancellations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);

    private async Task<HotelBookingCancellation> LoadCancellationAsync(
        HotelBookingCancellationId cancellationId,
        CancellationToken cancellationToken) =>
        await _db.HotelBookingCancellations
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == cancellationId, cancellationToken);

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
