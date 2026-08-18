using NodaTime;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned inbox for compensation-required delivery idempotency (at-least-once).
/// </summary>
public sealed class PaymentCompensationInboxRecord
{
    private PaymentCompensationInboxRecord()
    {
    }

    private PaymentCompensationInboxRecord(Guid paymentId, Instant processedAt)
    {
        PaymentId = paymentId;
        ProcessedAt = processedAt;
    }

    public Guid PaymentId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static PaymentCompensationInboxRecord Create(Guid paymentId, Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new PaymentCompensationInboxRecord(paymentId, now);
    }
}
