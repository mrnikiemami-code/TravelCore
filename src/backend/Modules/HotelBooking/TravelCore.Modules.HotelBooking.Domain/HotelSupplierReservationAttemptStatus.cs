namespace TravelCore.Modules.HotelBooking.Domain;

public enum HotelSupplierReservationAttemptStatus : short
{
    Created = 1,
    Initiated = 2,
    Confirmed = 3,
    Failed = 4,
}
