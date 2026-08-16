namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place catalog operational status (TC-P07-T004).
/// Catalog ops only — NOT delete/archive lifecycle (P07-R3 remains UNRESOLVED),
/// NOT live bookability / "bookable now" (Hotel Catalog ≠ Hotel Booking).
/// </summary>
public enum PlaceCatalogStatus : short
{
    /// <summary>Operator draft — not yet active in the catalog.</summary>
    Draft = 0,

    /// <summary>Active catalog entry (ops), not a booking availability signal.</summary>
    Active = 1,

    /// <summary>Inactive catalog entry (ops); not soft-delete / archive product.</summary>
    Inactive = 2
}
