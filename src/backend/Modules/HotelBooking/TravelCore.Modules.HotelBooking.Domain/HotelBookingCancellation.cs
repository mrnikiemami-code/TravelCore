using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned confirmed-cancellation process. Not HotelBookingStatus.
/// Tracks policy evaluation, supplier cancellation, and optional full Refund completion.
/// </summary>
public sealed class HotelBookingCancellation
{
    private readonly List<HotelSupplierCancellationAttempt> _attempts = [];

    private HotelBookingCancellation()
    {
        CurrencyCode = null!;
    }

    private HotelBookingCancellation(
        HotelBookingCancellationId id,
        HotelBookingId hotelBookingId,
        Guid paymentId,
        Instant requestedAt,
        HotelBookingCancellationFinancialOutcome financialOutcome,
        decimal penaltyAmount,
        decimal refundAmount,
        string currencyCode)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        PaymentId = paymentId;
        RequestedAt = requestedAt;
        Status = HotelBookingCancellationStatus.Requested;
        FinancialOutcome = financialOutcome;
        PenaltyAmount = penaltyAmount;
        RefundAmount = refundAmount;
        CurrencyCode = currencyCode;
        Version = 0;
    }

    public HotelBookingCancellationId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public Instant RequestedAt { get; private set; }

    public HotelBookingCancellationStatus Status { get; private set; }

    public HotelBookingCancellationFinancialOutcome FinancialOutcome { get; private set; }

    public decimal PenaltyAmount { get; private set; }

    public decimal RefundAmount { get; private set; }

    public string CurrencyCode { get; private set; }

    public Instant? CompletedAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<HotelSupplierCancellationAttempt> Attempts => _attempts;

    public bool HasUnresolvedAttempt => _attempts.Any(a => a.IsUnresolved);

    public bool RequiresFullRefund =>
        FinancialOutcome == HotelBookingCancellationFinancialOutcome.FullRefund;

    public static HotelBookingCancellation StartRequested(
        HotelBookingId hotelBookingId,
        Guid paymentId,
        Instant requestedAt,
        HotelCancellationPenaltyEvaluation evaluation)
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
                "HotelBookingCancellation can start only for executable FullRefund or NoRefund outcomes.");
        }

        var outcome = evaluation.Kind == HotelCancellationPenaltyEvaluationKind.FullRefund
            ? HotelBookingCancellationFinancialOutcome.FullRefund
            : HotelBookingCancellationFinancialOutcome.NoRefund;
        var penalty = evaluation.Penalty ?? throw new InvalidOperationException("Penalty is required.");
        var refund = evaluation.RefundAmount ?? throw new InvalidOperationException("RefundAmount is required.");

        return new HotelBookingCancellation(
            HotelBookingCancellationId.New(),
            hotelBookingId,
            paymentId,
            requestedAt,
            outcome,
            penalty.Amount,
            refund.Amount,
            penalty.Currency.Value);
    }

    public HotelSupplierCancellationAttempt StartAttempt(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelBookingCancellationStatus.Completed)
        {
            throw new InvalidOperationException("Completed cancellation cannot start another attempt.");
        }

        if (Status == HotelBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                "Supplier reservation is already cancelled; another cancellation attempt is forbidden.");
        }

        if (HasUnresolvedAttempt)
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated cancellation attempt blocks another attempt.");
        }

        if (_attempts.Any(a => a.Status == HotelSupplierCancellationAttemptStatus.Confirmed))
        {
            throw new InvalidOperationException(
                "Confirmed supplier cancellation forbids another cancellation attempt.");
        }

        var attempt = new HotelSupplierCancellationAttempt(
            HotelSupplierCancellationAttemptId.New(),
            Id,
            now);
        _attempts.Add(attempt);
        if (Status == HotelBookingCancellationStatus.Requested)
        {
            Status = HotelBookingCancellationStatus.SupplierCancellationPending;
        }

        IncrementVersion();
        return attempt;
    }

    public void MarkAttemptInitiated(HotelSupplierCancellationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkInitiated(now);
        if (Status == HotelBookingCancellationStatus.Requested)
        {
            Status = HotelBookingCancellationStatus.SupplierCancellationPending;
        }

        IncrementVersion();
    }

    public void ConfirmAttempt(HotelSupplierCancellationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkConfirmed(now);
        AdvanceAfterAuthoritativeSupplierCancellation(now);
        IncrementVersion();
    }

    public void FailAttempt(HotelSupplierCancellationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkFailed(now);
        IncrementVersion();
    }

    /// <summary>
    /// After HotelBooking is already Cancelled from authoritative supplier cancellation,
    /// matching RefundSucceeded completes FullRefund processing.
    /// </summary>
    public void CompleteFromAuthoritativeRefundSuccess(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelBookingCancellationStatus.Completed)
        {
            return;
        }

        if (Status != HotelBookingCancellationStatus.RefundPending)
        {
            throw new InvalidOperationException(
                $"Cancellation in status {Status} cannot complete from RefundSucceeded.");
        }

        if (!RequiresFullRefund)
        {
            throw new InvalidOperationException("NoRefund cancellation does not complete from RefundSucceeded.");
        }

        Status = HotelBookingCancellationStatus.Completed;
        CompletedAt = now;
        IncrementVersion();
    }

    public void AdvanceAfterAuthoritativeSupplierCancellation(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelBookingCancellationStatus.Completed
            || Status == HotelBookingCancellationStatus.RefundPending)
        {
            return;
        }

        if (RequiresFullRefund)
        {
            Status = HotelBookingCancellationStatus.RefundPending;
        }
        else
        {
            Status = HotelBookingCancellationStatus.Completed;
            CompletedAt = now;
        }
    }

    private HotelSupplierCancellationAttempt RequireAttempt(HotelSupplierCancellationAttemptId attemptId) =>
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
