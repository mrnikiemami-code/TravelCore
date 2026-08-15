namespace TravelCore.Modules.Identity.Domain;

/// <summary>
/// Account lifecycle. Not a soft-delete flag and not an Access authorization state.
/// </summary>
public enum AccountStatus : short
{
    Active = 1,
    Disabled = 2
}
