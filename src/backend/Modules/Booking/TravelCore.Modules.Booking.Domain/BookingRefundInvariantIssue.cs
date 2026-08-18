using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Durable evidence that RefundSucceeded arrived for a Booking that must not be cancelled (P20-R6).
/// </summary>
public sealed class BookingRefundInvariantIssue
{
    private BookingRefundInvariantIssue()
    {
    }

    private BookingRefundInvariantIssue(
        Guid id,
        BookingId bookingId,
        Guid refundId,
        Guid paymentId,
        BookingRefundInvariantIssueKind kind,
        Instant detectedAt)
    {
        Id = id;
        BookingId = bookingId;
        RefundId = refundId;
        PaymentId = paymentId;
        Kind = kind;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public BookingId BookingId { get; private set; }

    public Guid RefundId { get; private set; }

    public Guid PaymentId { get; private set; }

    public BookingRefundInvariantIssueKind Kind { get; private set; }

    public Instant DetectedAt { get; private set; }

    public static BookingRefundInvariantIssue Create(
        BookingId bookingId,
        Guid refundId,
        Guid paymentId,
        BookingRefundInvariantIssueKind kind,
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

        return new BookingRefundInvariantIssue(Uuid7.New(), bookingId, refundId, paymentId, kind, now);
    }
}
