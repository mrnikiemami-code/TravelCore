namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned reasons when Payment succeeded but Booking cannot confirm (P20-R5).
/// Not a Refund and not a PaymentStatus.
/// </summary>
public enum BookingConfirmationRecoveryReason
{
    ExpiredHold = 1,
    ReleasedHold = 2,
    CancelledBooking = 3,
    MonetaryMismatch = 4,
    MissingMonetarySnapshot = 5,
    MissingPeoplePrerequisites = 6,
}
