using NodaTime;
using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// FlightBooking-owned confirmed-cancellation process. Not FlightBookingStatus.
/// Tracks policy evaluation, supplier ticket/reservation reversal, and optional full Refund completion.
/// </summary>
public sealed class FlightBookingCancellation
{
    private readonly List<FlightSupplierReversalAttempt> _attempts = [];

    private FlightBookingCancellation()
    {
        CurrencyCode = null!;
    }

    private FlightBookingCancellation(
        FlightBookingCancellationId id,
        FlightBookingId flightBookingId,
        Guid paymentId,
        Instant requestedAt,
        FlightBookingCancellationFinancialOutcome financialOutcome,
        decimal penaltyAmount,
        decimal refundAmount,
        string currencyCode)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        PaymentId = paymentId;
        RequestedAt = requestedAt;
        Status = FlightBookingCancellationStatus.Requested;
        FinancialOutcome = financialOutcome;
        PenaltyAmount = penaltyAmount;
        RefundAmount = refundAmount;
        CurrencyCode = currencyCode;
        Version = 0;
    }

    public FlightBookingCancellationId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public Instant RequestedAt { get; private set; }

    public FlightBookingCancellationStatus Status { get; private set; }

    public FlightBookingCancellationFinancialOutcome FinancialOutcome { get; private set; }

    public decimal PenaltyAmount { get; private set; }

    public decimal RefundAmount { get; private set; }

    public string CurrencyCode { get; private set; }

    public Instant? CompletedAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<FlightSupplierReversalAttempt> Attempts => _attempts;

    public bool HasUnresolvedAttempt => _attempts.Any(a => a.IsUnresolved);

    public bool RequiresFullRefund =>
        FinancialOutcome == FlightBookingCancellationFinancialOutcome.FullRefund;

    public static FlightBookingCancellation StartRequested(
        FlightBookingId flightBookingId,
        Guid paymentId,
        Instant requestedAt,
        FlightCancellationPenaltyEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        if (requestedAt == default)
        {
            throw new ArgumentException("RequestedAt cannot be default.", nameof(requestedAt));
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (!evaluation.IsExecutable)
        {
            throw new InvalidOperationException(
                "FlightBookingCancellation can start only for executable FullRefund or NoRefund outcomes.");
        }

        var outcome = evaluation.Kind == FlightCancellationPenaltyEvaluationKind.FullRefund
            ? FlightBookingCancellationFinancialOutcome.FullRefund
            : FlightBookingCancellationFinancialOutcome.NoRefund;
        var penalty = evaluation.Penalty ?? throw new InvalidOperationException("Penalty is required.");
        var refund = evaluation.RefundAmount ?? throw new InvalidOperationException("RefundAmount is required.");

        return new FlightBookingCancellation(
            FlightBookingCancellationId.New(),
            flightBookingId,
            paymentId,
            requestedAt,
            outcome,
            penalty.Amount,
            refund.Amount,
            penalty.Currency.Value);
    }

    public FlightSupplierReversalAttempt StartAttempt(
        FlightSupplierReversalKind kind,
        Instant now,
        FlightTicketId? ticketId = null,
        FlightPassengerId? passengerId = null)
    {
        EnsureClock(now);
        if (Status == FlightBookingCancellationStatus.Completed)
        {
            throw new InvalidOperationException("Completed cancellation cannot start another attempt.");
        }

        if (Status == FlightBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                "Supplier reversal is already complete; another reversal attempt is forbidden.");
        }

        if (kind is FlightSupplierReversalKind.TicketVoid or FlightSupplierReversalKind.TicketRefund)
        {
            if (_attempts.Any(a => a.Kind == kind && a.TicketId.Equals(ticketId) && a.IsUnresolved))
            {
                throw new InvalidOperationException(
                    "An unresolved Created/Initiated ticket reversal blocks a duplicate of the same kind and ticket.");
            }

            if (_attempts.Any(a => a.Kind == kind
                && a.TicketId.Equals(ticketId)
                && a.Status == FlightSupplierReversalAttemptStatus.Succeeded))
            {
                throw new InvalidOperationException(
                    "Succeeded ticket reversal forbids another attempt of the same kind and ticket.");
            }
        }
        else if (kind == FlightSupplierReversalKind.ReservationCancel)
        {
            if (_attempts.Any(a => a.Kind == FlightSupplierReversalKind.ReservationCancel && a.IsUnresolved))
            {
                throw new InvalidOperationException(
                    "An unresolved Created/Initiated reservation cancel blocks another reservation cancel.");
            }

            if (_attempts.Any(a => a.Kind == FlightSupplierReversalKind.ReservationCancel
                && a.Status == FlightSupplierReversalAttemptStatus.Succeeded))
            {
                throw new InvalidOperationException(
                    "Succeeded reservation cancel forbids another reservation cancel attempt.");
            }
        }

        var attempt = new FlightSupplierReversalAttempt(
            FlightSupplierReversalAttemptId.New(),
            Id,
            kind,
            now,
            ticketId,
            passengerId);
        _attempts.Add(attempt);
        if (Status == FlightBookingCancellationStatus.Requested)
        {
            Status = FlightBookingCancellationStatus.SupplierReversalPending;
        }

        IncrementVersion();
        return attempt;
    }

    public void MarkAttemptInitiated(FlightSupplierReversalAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkInitiated(now);
        if (Status == FlightBookingCancellationStatus.Requested)
        {
            Status = FlightBookingCancellationStatus.SupplierReversalPending;
        }

        IncrementVersion();
    }

    public void SucceedAttempt(FlightSupplierReversalAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkSucceeded(now);
        if (Status == FlightBookingCancellationStatus.Requested)
        {
            Status = FlightBookingCancellationStatus.SupplierReversalPending;
        }

        IncrementVersion();
    }

    public void FailAttempt(FlightSupplierReversalAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkFailed(now);
        IncrementVersion();
    }

    /// <summary>
    /// After FlightBooking is already Cancelled from authoritative supplier reversal,
    /// matching RefundSucceeded completes FullRefund processing.
    /// </summary>
    public void CompleteFromAuthoritativeRefundSuccess(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightBookingCancellationStatus.Completed)
        {
            return;
        }

        if (Status != FlightBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                $"Cancellation in status {Status} cannot complete from RefundSucceeded.");
        }

        if (!RequiresFullRefund)
        {
            throw new InvalidOperationException("NoRefund cancellation does not complete from RefundSucceeded.");
        }

        Status = FlightBookingCancellationStatus.Completed;
        CompletedAt = now;
        IncrementVersion();
    }

    public void AdvanceAfterAuthoritativeCompleteReversal(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightBookingCancellationStatus.Completed
            || Status == FlightBookingCancellationStatus.RefundPending)
        {
            return;
        }

        if (RequiresFullRefund)
        {
            Status = FlightBookingCancellationStatus.RefundPending;
        }
        else
        {
            Status = FlightBookingCancellationStatus.Completed;
            CompletedAt = now;
        }

        IncrementVersion();
    }

    private FlightSupplierReversalAttempt RequireAttempt(FlightSupplierReversalAttemptId attemptId) =>
        _attempts.Single(a => a.Id.Equals(attemptId));

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Instant cannot be default.", nameof(now));
        }
    }

    private void IncrementVersion() => Version++;
}
