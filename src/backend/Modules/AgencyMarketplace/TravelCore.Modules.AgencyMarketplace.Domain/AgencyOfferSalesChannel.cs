namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Sales channel for an AgencyOffer (TC-P38-T003). Not a price and not Booking visibility.
/// </summary>
public enum AgencyOfferSalesChannel : short
{
    Public = 1,
    AgencyPortal = 2,
    Private = 3
}
