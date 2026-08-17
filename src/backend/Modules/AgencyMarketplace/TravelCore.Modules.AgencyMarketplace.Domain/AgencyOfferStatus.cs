namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace listing lifecycle. Not TourProduct catalog status and not Booking.
/// </summary>
public enum AgencyOfferStatus : short
{
    Draft = 1,
    Active = 2,
    Archived = 3
}
