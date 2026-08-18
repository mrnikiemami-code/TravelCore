using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned evidence that RefundSucceeded arrived in a state that must not auto-cancel (P21-R6).
/// Confirmed cancellation remains R7.
/// </summary>
public sealed class HotelBookingRefundInvariantIssue
{
    private HotelBookingRefundInvariantIssue()
    {
    }

    private HotelBookingRefundInvariantIssue(
        Guid id,
        HotelBookingId hotelBookingId,
        Guid refundId,
        Guid paymentId,
        HotelBookingRefundInvariantIssueKind kind,
        Instant detectedAt)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        RefundId = refundId;
        PaymentId = paymentId;
        Kind = kind;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public Guid RefundId { get; private set; }

    public Guid PaymentId { get; private set; }

    public HotelBookingRefundInvariantIssueKind Kind { get; private set; }

    public Instant DetectedAt { get; private set; }

    public static HotelBookingRefundInvariantIssue Create(
        HotelBookingId hotelBookingId,
        Guid refundId,
        Guid paymentId,
        HotelBookingRefundInvariantIssueKind kind,
        Instant now)
    {
        if (refundId == Guid.Empty)
        {
            throw new ArgumentException("RefundId cannot be empty.", nameof(refundId));
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("DetectedAt cannot be default.", nameof(now));
        }

        return new HotelBookingRefundInvariantIssue(
            Uuid7.New(),
            hotelBookingId,
            refundId,
            paymentId,
            kind,
            now);
    }
}

public enum HotelBookingRefundInvariantIssueKind : short
{
    ConfirmedBooking = 1,
    UnexpectedSupplierConfirmed = 2,
}
