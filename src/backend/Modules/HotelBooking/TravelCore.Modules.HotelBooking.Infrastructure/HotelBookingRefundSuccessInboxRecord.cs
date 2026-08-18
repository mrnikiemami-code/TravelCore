using NodaTime;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

public sealed class HotelBookingRefundSuccessInboxRecord
{
    private HotelBookingRefundSuccessInboxRecord()
    {
    }

    private HotelBookingRefundSuccessInboxRecord(Guid refundId, Instant processedAt)
    {
        RefundId = refundId;
        ProcessedAt = processedAt;
    }

    public Guid RefundId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static HotelBookingRefundSuccessInboxRecord Create(Guid refundId, Instant now)
    {
        if (refundId == Guid.Empty)
        {
            throw new ArgumentException("RefundId cannot be empty.", nameof(refundId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new HotelBookingRefundSuccessInboxRecord(refundId, now);
    }
}
