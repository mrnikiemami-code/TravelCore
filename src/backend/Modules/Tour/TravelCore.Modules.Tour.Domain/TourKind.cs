namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Closed TourKind classification (P09-R1).
/// One TourProduct has exactly one primary kind — Experience specialization starts in P10 (TC-P10-T001); Package specialty remains P11.
/// </summary>
public enum TourKind : short
{
    Experience = 1,
    Package = 2
}
