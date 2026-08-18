namespace TravelCore.Modules.Booking.Contracts;

/// <summary>
/// Opaque logical TourDeparture identifier. Booking does not clone TourDeparture (P19-R1).
/// Contains no schedule, capacity, TourProduct body, pricing, or availability facts.
/// </summary>
public readonly record struct TourDepartureReference(Guid LogicalId);
