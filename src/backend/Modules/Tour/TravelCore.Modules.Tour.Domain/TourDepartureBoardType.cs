namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Board / meal plan for a Departure accommodation option (P11-R6 · TC-P11-T006).
/// </summary>
public enum TourDepartureBoardType : short
{
    None = 0,
    Breakfast = 1,
    HalfBoard = 2,
    FullBoard = 3
}
