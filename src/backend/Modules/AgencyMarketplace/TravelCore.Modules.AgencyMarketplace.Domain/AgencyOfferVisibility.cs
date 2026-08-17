namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace listing visibility. Not SEO IndexPolicy and not Offer publish workflow (P13-R7).
/// </summary>
public enum AgencyOfferVisibility : short
{
    Unlisted = 1,
    Listed = 2
}
