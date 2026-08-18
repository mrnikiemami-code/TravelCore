namespace TravelCore.Modules.Flight.Domain;

public enum FlightSupplierReservationStatus : short
{
    Pending = 1,
    Confirmed = 2,
    Expired = 3,
    Cancelled = 4,
}
