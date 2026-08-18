using NodaTime;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

public sealed class HotelBookingPaymentSuccessInboxRecord
{
    private HotelBookingPaymentSuccessInboxRecord()
    {
    }

    private HotelBookingPaymentSuccessInboxRecord(Guid paymentId, Instant processedAt)
    {
        PaymentId = paymentId;
        ProcessedAt = processedAt;
    }

    public Guid PaymentId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static HotelBookingPaymentSuccessInboxRecord Create(Guid paymentId, Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new HotelBookingPaymentSuccessInboxRecord(paymentId, now);
    }
}
