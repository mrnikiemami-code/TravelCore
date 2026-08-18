namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned cancellation process states. Not HotelBookingStatus.
/// </summary>
public enum HotelBookingCancellationStatus : short
{
    Requested = 1,
    SupplierCancellationPending = 2,
    RefundPending = 3,
    Completed = 4,
}
