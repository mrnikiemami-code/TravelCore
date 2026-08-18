namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned traveler category. Not PlannerTravelerComposition counts (P19-R4).
/// Infant non-seat special handling is deferred.
/// </summary>
public enum TravelerCategory
{
    Adult = 1,
    Child = 2,
    Infant = 3,
}
