namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Flight passenger categories. Not HotelGuestCategory. Infant is first-class.
/// </summary>
public enum FlightPassengerCategory : short
{
    Adult = 1,
    Child = 2,
    Infant = 3,
}
