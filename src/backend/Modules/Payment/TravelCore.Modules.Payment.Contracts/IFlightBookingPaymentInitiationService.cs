namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trusted internal FlightBooking Payment initiation. Not a public API (P22-R6).
/// FlightBookingId is not an authorization credential.
/// </summary>
public interface IFlightBookingPaymentInitiationService
{
    Task<PaymentInitiationResult> InitiateForFlightBookingAsync(
        FlightBookingPaymentReference flightBooking,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}
