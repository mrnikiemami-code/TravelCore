namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// FlightBooking-owned cancellation process states. Not FlightBookingStatus.
/// </summary>
public enum FlightBookingCancellationStatus : short
{
    Requested = 1,
    SupplierReversalPending = 2,
    RefundPending = 3,
    Completed = 4,
}
