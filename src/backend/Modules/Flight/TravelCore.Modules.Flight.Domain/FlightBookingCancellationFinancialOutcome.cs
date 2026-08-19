namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Executable cancellation financial outcome. Not FlightBookingStatus.
/// Partial penalty is a request result, not a persisted process state.
/// </summary>
public enum FlightBookingCancellationFinancialOutcome : short
{
    FullRefund = 1,
    NoRefund = 2,
}
