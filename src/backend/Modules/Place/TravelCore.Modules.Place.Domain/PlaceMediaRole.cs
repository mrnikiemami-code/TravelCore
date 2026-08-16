namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Closed Place↔Media relationship roles (TC-P07-T005 architect lock).
/// Cover | Gallery only — no Hero/Thumbnail/Logo/Banner/opaque custom roles.
/// </summary>
public enum PlaceMediaRole : short
{
    Cover = 0,
    Gallery = 1
}
