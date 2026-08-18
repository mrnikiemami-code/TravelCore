namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned reasons that a successful Payment cannot complete the PayNow stay (P21-R6).
/// Not a HotelBookingStatus. Not emitted for handler crash, DB blip, timeout, or delayed delivery.
/// </summary>
public enum HotelBookingPaymentCompensationReason : short
{
    HoldExpired = 1,
    HoldReleased = 2,
    SupplierReservationNotCreated = 3,
    SupplierReservationCancelled = 4,
    MonetaryMismatch = 5,
    CurrencyMismatch = 6,
    RoomSetMismatch = 7,
    StayMismatch = 8,
    HotelMismatch = 9,
    CancellationTermsMismatch = 10,
}
