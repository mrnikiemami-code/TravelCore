namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trusted internal HotelBooking Payment initiation. Not a public API (P21-R6 / R8).
/// HotelBookingId is not an authorization credential.
/// </summary>
public interface IHotelBookingPaymentInitiationService
{
    Task<PaymentInitiationResult> InitiateForHotelBookingAsync(
        HotelBookingPaymentReference hotelBooking,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}
