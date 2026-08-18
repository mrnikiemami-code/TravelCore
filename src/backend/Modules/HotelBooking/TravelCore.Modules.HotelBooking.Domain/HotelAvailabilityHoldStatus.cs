namespace TravelCore.Modules.HotelBooking.Domain;

public enum HotelAvailabilityHoldStatus : short
{
    Requested = 1,
    Active = 2,
    Released = 3,
    Expired = 4,
}
