using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned durable evidence that Payment has authoritatively succeeded for this stay (P21-R6).
/// Not confirmation. Unique per HotelBooking.
/// </summary>
public sealed class HotelBookingPaymentEvidence
{
    private HotelBookingPaymentEvidence()
    {
        CurrencyCode = null!;
    }

    private HotelBookingPaymentEvidence(
        HotelBookingId hotelBookingId,
        Guid paymentId,
        decimal amount,
        string currencyCode,
        Instant verifiedAt)
    {
        HotelBookingId = hotelBookingId;
        PaymentId = paymentId;
        Amount = amount;
        CurrencyCode = currencyCode;
        VerifiedAt = verifiedAt;
    }

    public HotelBookingId HotelBookingId { get; private set; }

    public Guid PaymentId { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; }

    public Instant VerifiedAt { get; private set; }

    public static HotelBookingPaymentEvidence Record(
        HotelBookingId hotelBookingId,
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

        return new HotelBookingPaymentEvidence(
            hotelBookingId,
            paymentId,
            amount,
            currencyCode.Trim().ToUpperInvariant(),
            now);
    }

    public bool MatchesMonetarySnapshot(HotelBookingMonetarySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return Amount == snapshot.Total.Amount
            && string.Equals(CurrencyCode, snapshot.CurrencyCode.Value, StringComparison.Ordinal);
    }
}
