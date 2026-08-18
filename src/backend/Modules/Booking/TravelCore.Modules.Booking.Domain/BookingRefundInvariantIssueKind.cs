namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Minimal Booking-owned invariant when RefundSucceeded cannot cancel the Booking (P20-R6).
/// </summary>
public enum BookingRefundInvariantIssueKind
{
    ConfirmedBooking = 1,
}
