namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourProduct catalog publication status (P09-R4).
/// Catalog-visible when Published — not bookable, not Index, not delete/archive.
/// </summary>
public enum TourCatalogStatus : short
{
    Draft = 0,
    Published = 1,
    Inactive = 2
}
