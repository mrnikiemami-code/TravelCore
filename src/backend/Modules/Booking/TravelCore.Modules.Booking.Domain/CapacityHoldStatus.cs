namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned capacity-consumption lifecycle (TC-P19-T003 / P19-R3).
/// Distinct from BookingStatus. Expired here is hold expiry, not Booking Expired.
/// </summary>
public enum CapacityHoldStatus
{
    Active = 1,
    Consumed = 2,
    Released = 3,
    Expired = 4,
}
