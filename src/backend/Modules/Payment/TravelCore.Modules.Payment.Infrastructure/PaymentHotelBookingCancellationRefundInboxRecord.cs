using NodaTime;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned inbox for HotelBooking confirmed-cancellation full-refund delivery (P21-R7).
/// Keyed by HotelBookingCancellationId. No HotelBooking schema FK.
/// </summary>
public sealed class PaymentHotelBookingCancellationRefundInboxRecord
{
    private PaymentHotelBookingCancellationRefundInboxRecord()
    {
    }

    private PaymentHotelBookingCancellationRefundInboxRecord(
        Guid hotelBookingCancellationId,
        Guid paymentId,
        Instant processedAt)
    {
        HotelBookingCancellationId = hotelBookingCancellationId;
        PaymentId = paymentId;
        ProcessedAt = processedAt;
    }

    public Guid HotelBookingCancellationId { get; private set; }

    public Guid PaymentId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static PaymentHotelBookingCancellationRefundInboxRecord Create(
        Guid hotelBookingCancellationId,
        Guid paymentId,
        Instant now)
    {
        if (hotelBookingCancellationId == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingCancellationId cannot be empty.", nameof(hotelBookingCancellationId));
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new PaymentHotelBookingCancellationRefundInboxRecord(
            hotelBookingCancellationId,
            paymentId,
            now);
    }
}
