namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Reasons a successful Payment cannot complete the FlightBooking (P22-R6).
/// Ambiguous ticketing is not a compensation reason.
/// </summary>
public enum FlightBookingPaymentCompensationReason : short
{
    ReservationExpired = 1,
    ReservationCancelled = 2,
    TicketingDeadlineExpired = 3,
    TicketingDefinitivelyFailed = 4,
}
