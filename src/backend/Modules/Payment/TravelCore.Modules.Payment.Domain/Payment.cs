using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Logical monetary collection for exactly one Booking (TC-P20-T002 / P20-R2).
/// Owns PaymentAttempt children. Does not own Booking, Pricing, or Refund.
/// </summary>
public sealed class Payment
{
    private readonly List<PaymentAttempt> _attempts = [];

    private Payment()
    {
    }

    private Payment(PaymentId id, BookingReference booking, Instant createdAt)
    {
        Id = id;
        Booking = booking;
        Status = PaymentStatus.Pending;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
    }

    public PaymentId Id { get; private set; }

    public BookingReference Booking { get; private set; }

    public PaymentStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public Instant? SucceededAt { get; private set; }

    public IReadOnlyList<PaymentAttempt> Attempts => _attempts;

    public static Payment Create(BookingReference booking, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new Payment(PaymentId.New(), booking, now);
    }

    public PaymentAttempt CreateAttempt(Instant now)
    {
        EnsureClock(now);
        EnsurePending();
        if (_attempts.Any(attempt => attempt.IsActive))
        {
            throw new InvalidOperationException(
                "A Payment may have at most one non-terminal PaymentAttempt at a time.");
        }

        var attempt = new PaymentAttempt(PaymentAttemptId.New(), now);
        _attempts.Add(attempt);
        return attempt;
    }

    public void InitiateAttempt(PaymentAttemptId attemptId, Instant now)
    {
        EnsurePending();
        FindAttempt(attemptId).MarkInitiated(now);
    }

    public void RecordAttemptFailure(PaymentAttemptId attemptId, Instant now)
    {
        EnsurePending();
        FindAttempt(attemptId).RecordFailure(now);
    }

    /// <summary>
    /// Trusted-evidence boundary for authoritative collection success.
    /// Not client/browser-controlled. P20-R3 owns real provider verification.
    /// </summary>
    internal void RecordAuthoritativeCollectionSuccess(PaymentAttemptId attemptId, Instant now)
    {
        EnsureClock(now);
        EnsurePending();
        if (_attempts.Any(attempt => attempt.Status == PaymentAttemptStatus.Succeeded))
        {
            throw new InvalidOperationException("A Payment may have at most one successful PaymentAttempt.");
        }

        FindAttempt(attemptId).RecordAuthoritativeSuccess(now);
        Status = PaymentStatus.Succeeded;
        StatusChangedAt = now;
        SucceededAt = now;
    }

    private PaymentAttempt FindAttempt(PaymentAttemptId attemptId)
    {
        var attempt = _attempts.SingleOrDefault(item => item.Id.Equals(attemptId));
        if (attempt is null)
        {
            throw new InvalidOperationException("PaymentAttempt does not belong to this Payment.");
        }

        return attempt;
    }

    private void EnsurePending()
    {
        if (Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException(
                "Succeeded Payment cannot accept further collection attempts or status reversal.");
        }

        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Payment is not Pending.");
        }
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
