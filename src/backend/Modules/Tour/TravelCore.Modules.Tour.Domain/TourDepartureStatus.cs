namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourDeparture execution lifecycle status (P11-R4 · TC-P11-T004).
/// Not SEO IndexPolicy, not TourProduct CatalogStatus, not Booking/Payment status.
/// </summary>
public enum TourDepartureStatus : short
{
    Draft = 0,
    Published = 1,
    Closed = 2,
    Cancelled = 3,
    Completed = 4
}
