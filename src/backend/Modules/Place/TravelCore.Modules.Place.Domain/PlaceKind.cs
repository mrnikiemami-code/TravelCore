namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Closed PlaceKind classification (P07-R1).
/// One Place has exactly one primary kind — no multi-kind Places.
/// </summary>
public enum PlaceKind : short
{
    Hotel = 1,
    Restaurant = 2,
    Attraction = 3
}
