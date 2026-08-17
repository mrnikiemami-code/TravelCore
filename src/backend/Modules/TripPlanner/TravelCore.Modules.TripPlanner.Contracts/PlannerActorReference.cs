namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// Opaque logical actor/account reference for optional authenticated TripPlanner association (P18-R3).
/// Not a User, Account, Party, Person, or Customer master record.
/// </summary>
public readonly record struct PlannerActorReference(Guid ActorId);
