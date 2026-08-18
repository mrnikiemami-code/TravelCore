namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// HotelBooking-owned trusted payable obligation for Payment preparation (P21-R6).
/// Source is HotelBookingMonetarySnapshot. Excludes guest/contact PII.
/// </summary>
public sealed record HotelBookingPaymentObligationRead(
    Guid HotelBookingId,
    string HotelBookingStatus,
    decimal Amount,
    string CurrencyCode,
    Guid SnapshotId,
    bool PaymentEligible);

public interface IHotelBookingPaymentObligationQuery
{
    Task<HotelBookingPaymentObligationRead?> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default);
}
