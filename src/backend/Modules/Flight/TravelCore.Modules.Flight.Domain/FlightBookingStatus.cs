namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// FlightBooking lifecycle for P22-R6. New bookings start Pending.
/// Confirmed requires reservation + Payment + complete issued tickets.
/// Cancelled is Pending-only compensation after full Refund (R6) or Confirmed supplier reversal (R7).
/// </summary>
public enum FlightBookingStatus : short
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
}
