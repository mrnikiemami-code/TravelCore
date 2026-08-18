using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Durable evidence that Payment succeeded and HotelBooking cannot be confirmed (P21-R6).
/// Separate from HotelBookingStatus, PaymentStatus, and Refund.
/// </summary>
public sealed class HotelBookingPaymentCompensationEvidence
{
    private HotelBookingPaymentCompensationEvidence()
    {
    }

    private HotelBookingPaymentCompensationEvidence(
        Guid id,
        HotelBookingId hotelBookingId,
        Guid paymentId,
        HotelBookingPaymentCompensationReason reason,
        Instant detectedAt)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        PaymentId = paymentId;
        Reason = reason;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public HotelBookingPaymentCompensationReason Reason { get; private set; }

    public Instant DetectedAt { get; private set; }

    public static HotelBookingPaymentCompensationEvidence Create(
        HotelBookingId hotelBookingId,
        Guid paymentId,
        HotelBookingPaymentCompensationReason reason,
        Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("DetectedAt cannot be default.", nameof(now));
        }

        return new HotelBookingPaymentCompensationEvidence(Uuid7.New(), hotelBookingId, paymentId, reason, now);
    }
}
