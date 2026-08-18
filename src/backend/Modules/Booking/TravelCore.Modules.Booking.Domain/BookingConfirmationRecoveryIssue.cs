using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned durable evidence that Payment succeeded and confirmation was refused.
/// Separate from PaymentStatus, BookingStatus, PaymentReconciliationIssue, and Refund (P20-R5).
/// </summary>
public sealed class BookingConfirmationRecoveryIssue
{
    private BookingConfirmationRecoveryIssue()
    {
    }

    private BookingConfirmationRecoveryIssue(
        Guid id,
        BookingId bookingId,
        Guid paymentId,
        BookingConfirmationRecoveryReason reason,
        Instant detectedAt)
    {
        Id = id;
        BookingId = bookingId;
        PaymentId = paymentId;
        Reason = reason;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public BookingId BookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public BookingConfirmationRecoveryReason Reason { get; private set; }

    public Instant DetectedAt { get; private set; }

    public static BookingConfirmationRecoveryIssue Create(
        BookingId bookingId,
        Guid paymentId,
        BookingConfirmationRecoveryReason reason,
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

        return new BookingConfirmationRecoveryIssue(Uuid7.New(), bookingId, paymentId, reason, now);
    }
}
