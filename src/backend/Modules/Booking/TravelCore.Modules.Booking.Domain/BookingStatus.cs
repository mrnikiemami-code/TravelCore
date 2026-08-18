namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Minimal Booking-owned lifecycle (TC-P19-T002 / P19-R2).
/// Not PaymentStatus, capacity status, or QuoteStatus.
/// </summary>
public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
}
