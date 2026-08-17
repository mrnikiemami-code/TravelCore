namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R7: Public AgencyOffer presentation is composition only.
/// Agency Marketplace owns offer facts / publication / commercial relationship.
/// AgencyOffer may be displayed but does not own commercial flow.
/// Visibility does not change CatalogStatus / SEO IndexPolicy / canonical.
/// </summary>
public static class PublicExperienceAgencyOfferBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "AgencyMarketplace";
    public const string IndexPolicyOwner = "Seo";
    public const string CatalogStatusOwner = "Tour";
    public const string CompositionPosture = "InquiryOrientedAgencyInformation";
    public const bool CommercialFlowAllowed = false;
    public const bool AgencyPriceDisplayAllowed = false;
    public const bool RankingAllowed = false;
    public const bool BookingCtaAllowed = false;
    public const int MaxItems = 6;
}
