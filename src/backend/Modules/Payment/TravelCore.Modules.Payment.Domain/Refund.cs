using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// One logical full-return obligation for a successful Payment (TC-P20-T006 / P20-R6).
/// Amount/currency are copied from PaymentExecutionSnapshot and are immutable.
/// Successful Refund does not rewrite PaymentStatus.Succeeded.
/// Target is copied from Payment (exactly one of Tour Booking or HotelBooking).
/// </summary>
public sealed class Refund
{
    private readonly List<RefundAttempt> _attempts = [];

    private Refund()
    {
        Amount = null!;
    }

    private Refund(
        RefundId id,
        PaymentId paymentId,
        BookingReference? booking,
        HotelBookingPaymentReference? hotelBooking,
        MoneyValue amount,
        Instant createdAt)
    {
        if (booking is null == hotelBooking is null)
        {
            throw new ArgumentException("A Refund must belong to exactly one supported target.");
        }

        Id = id;
        PaymentId = paymentId;
        Booking = booking;
        HotelBooking = hotelBooking;
        Amount = amount;
        Status = RefundStatus.Pending;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
        Version = 0;
    }

    public RefundId Id { get; private set; }

    public PaymentId PaymentId { get; private set; }

    public BookingReference? Booking { get; private set; }

    public HotelBookingPaymentReference? HotelBooking { get; private set; }

    public PaymentTargetKind TargetKind =>
        HotelBooking is not null ? PaymentTargetKind.HotelBooking : PaymentTargetKind.TourBooking;

    public Guid TargetReferenceId =>
        HotelBooking?.HotelBookingId
        ?? Booking?.BookingId
        ?? throw new InvalidOperationException("Refund has no target.");

    public MoneyValue Amount { get; private set; }

    public RefundStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public Instant? SucceededAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<RefundAttempt> Attempts => _attempts;

    public static Refund CreateForSucceededPayment(
        Payment payment,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(payment);
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        if (payment.Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("A Refund may be created only for a Succeeded Payment.");
        }

        if (payment.ExecutionSnapshot is null)
        {
            throw new InvalidOperationException("Refund requires PaymentExecutionSnapshot.");
        }

        return new Refund(
            RefundId.New(),
            payment.Id,
            payment.Booking,
            payment.HotelBooking,
            payment.ExecutionSnapshot.Amount,
            now);
    }

    public RefundAttempt CreateAttempt(Instant now)
    {
        EnsureClock(now);
        if (Status == RefundStatus.Succeeded)
        {
            throw new InvalidOperationException("Succeeded Refund cannot start another RefundAttempt.");
        }

        if (_attempts.Any(attempt => attempt.IsActive))
        {
            throw new InvalidOperationException(
                "A Refund may have at most one non-terminal RefundAttempt at a time.");
        }

        var attempt = new RefundAttempt(RefundAttemptId.New(), now);
        _attempts.Add(attempt);
        IncrementVersion();
        return attempt;
    }

    public void RecordProviderInitiation(
        RefundAttemptId attemptId,
        Instant now,
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        EnsureClock(now);
        EnsurePending();
        var attempt = FindAttempt(attemptId);
        attempt.AttachProviderCorrelation(providerKey, requestReference, transactionReference);
        attempt.MarkInitiated(now);
        IncrementVersion();
    }

    public void RecordAmbiguousProviderInitiation(
        RefundAttemptId attemptId,
        Instant now,
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        EnsureClock(now);
        EnsurePending();
        var attempt = FindAttempt(attemptId);
        attempt.AttachProviderCorrelation(providerKey, requestReference, transactionReference);
        IncrementVersion();
    }

    public void RecordAttemptFailure(RefundAttemptId attemptId, Instant now)
    {
        EnsureClock(now);
        EnsurePending();
        FindAttempt(attemptId).RecordFailure(now);
        IncrementVersion();
    }

    public void RecordAuthoritativeRefundSuccess(RefundAttemptId attemptId, Instant now)
    {
        EnsureClock(now);
        var attempt = FindAttempt(attemptId);
        if (Status == RefundStatus.Succeeded && attempt.Status == RefundAttemptStatus.Succeeded)
        {
            return;
        }

        EnsurePending();
        if (_attempts.Any(item => item.Status == RefundAttemptStatus.Succeeded && !item.Id.Equals(attemptId)))
        {
            throw new InvalidOperationException("A Refund may have at most one successful RefundAttempt.");
        }

        attempt.RecordAuthoritativeSuccess(now);
        Status = RefundStatus.Succeeded;
        SucceededAt = now;
        StatusChangedAt = now;
        IncrementVersion();
    }

    private RefundAttempt FindAttempt(RefundAttemptId attemptId)
    {
        return _attempts.SingleOrDefault(item => item.Id.Equals(attemptId))
            ?? throw new InvalidOperationException("RefundAttempt was not found.");
    }

    private void EnsurePending()
    {
        if (Status != RefundStatus.Pending)
        {
            throw new InvalidOperationException("Only a Pending Refund can accept new execution.");
        }
    }

    private void IncrementVersion() => Version++;

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
