namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Executable cancellation financial outcome. Not HotelBookingStatus.
/// Partial penalty is a request result, not a persisted process state.
/// </summary>
public enum HotelBookingCancellationFinancialOutcome : short
{
    FullRefund = 1,
    NoRefund = 2,
}
