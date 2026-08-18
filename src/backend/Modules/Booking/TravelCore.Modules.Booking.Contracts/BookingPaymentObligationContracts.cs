namespace TravelCore.Modules.Booking.Contracts;

/// <summary>
/// Booking-owned trusted payable obligation for Payment preparation (P20-R5).
/// Contract is read-only and excludes passenger/contact PII.
/// </summary>
public sealed record BookingPaymentObligationRead(
    Guid BookingId,
    string BookingStatus,
    decimal Amount,
    string CurrencyCode,
    Guid SnapshotId,
    bool PaymentEligible);

public interface IBookingPaymentObligationQuery
{
    Task<BookingPaymentObligationRead?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);
}
