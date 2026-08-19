using NodaTime;

namespace TravelCore.Modules.Flight.Infrastructure;

public sealed class FlightRefundSuccessInboxRecord
{
    private FlightRefundSuccessInboxRecord()
    {
    }

    private FlightRefundSuccessInboxRecord(Guid refundId, Instant processedAt)
    {
        RefundId = refundId;
        ProcessedAt = processedAt;
    }

    public Guid RefundId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static FlightRefundSuccessInboxRecord Create(Guid refundId, Instant now)
    {
        if (refundId == Guid.Empty)
        {
            throw new ArgumentException("RefundId cannot be empty.", nameof(refundId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new FlightRefundSuccessInboxRecord(refundId, now);
    }
}
