namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace sales-availability metadata (TC-P13-T005 / P13-R5).
/// Not seat inventory, not TourDeparture capacity, not Booking reservation.
/// </summary>
public sealed class AgencyOfferSalesAvailability
{
    private AgencyOfferSalesAvailability()
    {
    }

    public AgencyOfferSalesAvailability(bool salesOpen)
    {
        SalesOpen = salesOpen;
    }

    /// <summary>Commercial sales window intent. Does not mean seats are available.</summary>
    public bool SalesOpen { get; private set; }

    public static AgencyOfferSalesAvailability Closed() => new(salesOpen: false);

    public static AgencyOfferSalesAvailability Open() => new(salesOpen: true);
}
