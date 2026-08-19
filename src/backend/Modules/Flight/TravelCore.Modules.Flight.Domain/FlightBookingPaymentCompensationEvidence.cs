using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Durable evidence that Payment succeeded and FlightBooking cannot be confirmed (P22-R6).
/// </summary>
public sealed class FlightBookingPaymentCompensationEvidence
{
    private FlightBookingPaymentCompensationEvidence()
    {
    }

    private FlightBookingPaymentCompensationEvidence(
        Guid id,
        FlightBookingId flightBookingId,
        Guid paymentId,
        FlightBookingPaymentCompensationReason reason,
        Instant detectedAt)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        PaymentId = paymentId;
        Reason = reason;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public FlightBookingPaymentCompensationReason Reason { get; private set; }

    public Instant DetectedAt { get; private set; }

    public static FlightBookingPaymentCompensationEvidence Create(
        FlightBookingId flightBookingId,
        Guid paymentId,
        FlightBookingPaymentCompensationReason reason,
        Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason), reason, "Compensation reason is not controlled.");
        }

        if (now == default)
        {
            throw new ArgumentException("DetectedAt cannot be default.", nameof(now));
        }

        return new FlightBookingPaymentCompensationEvidence(Uuid7.New(), flightBookingId, paymentId, reason, now);
    }
}
