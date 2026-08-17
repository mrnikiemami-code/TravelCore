namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// Opaque logical product/geography reference for future planner preferences.
/// TripPlanner does not own Destination/Tour/Place aggregates (P18-R1).
/// </summary>
public readonly record struct TripPlannerLogicalReference(Guid LogicalId);
