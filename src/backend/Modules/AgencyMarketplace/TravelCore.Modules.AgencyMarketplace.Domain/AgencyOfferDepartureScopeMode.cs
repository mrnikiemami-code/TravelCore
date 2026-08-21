namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Departure scope mode for an AgencyOffer (TC-P38-T003).
/// ALL = any departure of the TourProduct; Listed = explicit logical departure ids.
/// Does not own capacity / inventory.
/// </summary>
public enum AgencyOfferDepartureScopeMode : short
{
    All = 1,
    Listed = 2
}
