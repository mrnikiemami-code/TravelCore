namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Opaque logical actor id for future UGC authorship.
/// UGC does not own Identity/Party/User entities (P16-R1).
/// </summary>
public readonly record struct UgcActorReference(Guid ActorId);
