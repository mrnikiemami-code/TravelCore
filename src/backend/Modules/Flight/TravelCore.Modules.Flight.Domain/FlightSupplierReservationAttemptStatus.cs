namespace TravelCore.Modules.Flight.Domain;

public enum FlightSupplierReservationAttemptStatus : short
{
    Created = 1,
    Initiated = 2,
    Confirmed = 3,
    Failed = 4,
}
