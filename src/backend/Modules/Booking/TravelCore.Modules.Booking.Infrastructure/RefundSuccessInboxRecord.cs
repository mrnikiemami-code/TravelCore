using NodaTime;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Booking-owned inbox for RefundSucceeded delivery idempotency (at-least-once outbox).
/// </summary>
public sealed class RefundSuccessInboxRecord
{
    private RefundSuccessInboxRecord()
    {
    }

    private RefundSuccessInboxRecord(Guid refundId, Instant processedAt)
    {
        RefundId = refundId;
        ProcessedAt = processedAt;
    }

    public Guid RefundId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static RefundSuccessInboxRecord Create(Guid refundId, Instant now)
    {
        if (refundId == Guid.Empty)
        {
            throw new ArgumentException("RefundId cannot be empty.", nameof(refundId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new RefundSuccessInboxRecord(refundId, now);
    }
}
