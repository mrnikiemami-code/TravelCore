namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Explicit Party lifecycle. Not a global soft-delete flag.
/// </summary>
public enum PartyStatus : short
{
    Active = 1,
    Inactive = 2
}
