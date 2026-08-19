namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Flight-owned trusted payable obligation for Payment preparation (P22-R6).
/// Eligible only after Confirmed supplier reservation and before ticketing/reservation expiry.
/// </summary>
public sealed record FlightBookingPaymentObligationRead(
    Guid FlightBookingId,
    string FlightBookingStatus,
    decimal Amount,
    string CurrencyCode,
    Guid SnapshotId,
    bool PaymentEligible);

public interface IFlightBookingPaymentObligationQuery
{
    Task<FlightBookingPaymentObligationRead?> GetByFlightBookingIdAsync(
        Guid flightBookingId,
        CancellationToken cancellationToken = default);
}
