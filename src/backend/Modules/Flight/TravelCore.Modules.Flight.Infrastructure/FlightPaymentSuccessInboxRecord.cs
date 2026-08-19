using NodaTime;

namespace TravelCore.Modules.Flight.Infrastructure;

public sealed class FlightPaymentSuccessInboxRecord
{
    private FlightPaymentSuccessInboxRecord()
    {
    }

    private FlightPaymentSuccessInboxRecord(Guid paymentId, Instant processedAt)
    {
        PaymentId = paymentId;
        ProcessedAt = processedAt;
    }

    public Guid PaymentId { get; private set; }

    public Instant ProcessedAt { get; private set; }

    public static FlightPaymentSuccessInboxRecord Create(Guid paymentId, Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        return new FlightPaymentSuccessInboxRecord(paymentId, now);
    }
}
