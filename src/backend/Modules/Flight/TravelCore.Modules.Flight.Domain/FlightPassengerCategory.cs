namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight passenger categories for P22-R2. Not HotelGuestCategory. Infant is first-class.
/// </summary>
public enum FlightPassengerCategory : short
{
    Adult = 1,
    Child = 2,
    Infant = 3,
}
