namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace publication/moderation lifecycle for an AgencyOffer (TC-P13-T007 / P13-R7).
/// Not TourProduct catalog status and not SEO IndexPolicy.
/// </summary>
public enum AgencyOfferPublicationStatus : short
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Published = 4,
    Rejected = 5,
    Archived = 6
}
