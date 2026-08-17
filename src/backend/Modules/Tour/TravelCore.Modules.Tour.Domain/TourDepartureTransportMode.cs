namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Descriptive transport mode for a Departure segment (P11-R5 · TC-P11-T005).
/// Not a Flight domain type.
/// </summary>
public enum TourDepartureTransportMode : short
{
    Air = 0,
    Ground = 1,
    Other = 2
}
