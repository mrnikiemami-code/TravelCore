namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Closed TourKind classification (P09-R1).
/// One TourProduct has exactly one primary kind — typed specialization tables deferred (P09-R7 → P10/P11).
/// </summary>
public enum TourKind : short
{
    Experience = 1,
    Package = 2
}
