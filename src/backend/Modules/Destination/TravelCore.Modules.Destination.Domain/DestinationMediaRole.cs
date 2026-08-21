namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Closed Destination↔Media relationship roles (TC-P32-T008 Option A).
/// Cover only — Gallery deferred; no Hero/Thumbnail/Logo/Banner/opaque custom roles.
/// </summary>
public enum DestinationMediaRole : short
{
    Cover = 0
}
