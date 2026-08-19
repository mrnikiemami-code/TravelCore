using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight-owned durable evidence that Payment has authoritatively succeeded (P22-R6).
/// Not confirmation. Unique per FlightBooking.
/// </summary>
public sealed class FlightBookingPaymentEvidence
{
    private FlightBookingPaymentEvidence()
    {
        CurrencyCode = null!;
    }

    private FlightBookingPaymentEvidence(
        FlightBookingId flightBookingId,
        Guid paymentId,
        decimal amount,
        string currencyCode,
        Instant verifiedAt)
    {
        FlightBookingId = flightBookingId;
        PaymentId = paymentId;
        Amount = amount;
        CurrencyCode = currencyCode;
        VerifiedAt = verifiedAt;
    }

    public FlightBookingId FlightBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; }

    public Instant VerifiedAt { get; private set; }

    public static FlightBookingPaymentEvidence Record(
        FlightBookingId flightBookingId,
        Guid paymentId,
        decimal amount,
        string currencyCode,
        Instant now)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        if (now == default)
        {
            throw new ArgumentException("VerifiedAt cannot be default.", nameof(now));
        }

        return new FlightBookingPaymentEvidence(
            flightBookingId,
            paymentId,
            amount,
            currencyCode.Trim().ToUpperInvariant(),
            now);
    }

    public bool MatchesMonetarySnapshot(FlightBookingMonetarySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return Amount == snapshot.Total.Amount
            && string.Equals(CurrencyCode, snapshot.CurrencyCode.Value, StringComparison.Ordinal);
    }
}
