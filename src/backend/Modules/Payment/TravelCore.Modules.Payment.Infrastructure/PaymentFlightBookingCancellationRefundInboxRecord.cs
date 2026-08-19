using NodaTime;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned inbox for FlightBooking confirmed-cancellation full-refund delivery (P22-R7).
/// Keyed by FlightBookingCancellationId. No Flight schema FK.
/// </summary>
public sealed class PaymentFlightBookingCancellationRefundInboxRecord
{
    private PaymentFlightBookingCancellationRefundInboxRecord()
    {
    }

    private PaymentFlightBookingCancellationRefundInboxRecord(
        Guid flightBookingCancellationId,
        Guid paymentId,
        Instant processedAt)
    {
        FlightBookingCancellationId = flightBookingCancellationId;
        PaymentId = paymentId;
        ProcessedAt = processedAt;
    }

    public Guid FlightBookingCancellationId { get; private set; }

    public Guid PaymentId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static PaymentFlightBookingCancellationRefundInboxRecord Create(
        Guid flightBookingCancellationId,
        Guid paymentId,
        Instant now)
    {
        if (flightBookingCancellationId == Guid.Empty)
        {
            throw new ArgumentException(
                "FlightBookingCancellationId cannot be empty.",
                nameof(flightBookingCancellationId));
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new PaymentFlightBookingCancellationRefundInboxRecord(
            flightBookingCancellationId,
            paymentId,
            now);
    }
}
