namespace TravelCore.Modules.Access.Infrastructure.Authorization;

public static class AccessAuthorizationPolicies
{
    /// <summary>
    /// Admin sample policy: Access-backed <c>access.roles.read</c>.
    /// </summary>
    public const string AdminRolesRead = "Access.Admin.Roles.Read";

    /// <summary>
    /// Agency presentation sample policy: Access-backed <c>agency.panel.open</c>.
    /// </summary>
    public const string AgencyPanelOpen = "Access.Agency.Panel.Open";

    /// <summary>
    /// Destination Admin mutations: Access-backed <c>destination.destinations.write</c>.
    /// </summary>
    public const string DestinationDestinationsWrite = "Access.Destination.Destinations.Write";

    /// <summary>
    /// Destination SEO posture mutations: Access-backed <c>seo.destination-posture.write</c>.
    /// </summary>
    public const string SeoDestinationPostureWrite = "Access.Seo.DestinationPosture.Write";

    /// <summary>
    /// Media Admin upload/mutations: Access-backed <c>media.assets.write</c>.
    /// </summary>
    public const string MediaAssetsWrite = "Access.Media.Assets.Write";

    /// <summary>
    /// Place Admin catalog mutations: Access-backed <c>place.places.write</c>.
    /// </summary>
    public const string PlacePlacesWrite = "Access.Place.Places.Write";

    /// <summary>
    /// Place SEO route publication: Access-backed <c>seo.place-posture.write</c>.
    /// </summary>
    public const string SeoPlacePostureWrite = "Access.Seo.PlacePosture.Write";

    /// <summary>
    /// Content Admin editorial mutations: Access-backed <c>content.items.write</c>.
    /// </summary>
    public const string ContentItemsWrite = "Access.Content.Items.Write";

    /// <summary>
    /// Content SEO route publication: Access-backed <c>seo.content-posture.write</c>.
    /// </summary>
    public const string SeoContentPostureWrite = "Access.Seo.ContentPosture.Write";

    /// <summary>
    /// Tour Admin catalog mutations: Access-backed <c>tour.products.write</c>.
    /// </summary>
    public const string TourProductsWrite = "Access.Tour.Products.Write";

    /// <summary>
    /// TourDeparture Admin reads: Access-backed <c>tour.departures.read</c>.
    /// </summary>
    public const string TourDeparturesRead = "Access.Tour.Departures.Read";

    /// <summary>
    /// TourDeparture Admin mutations: Access-backed <c>tour.departures.write</c>.
    /// </summary>
    public const string TourDeparturesWrite = "Access.Tour.Departures.Write";

    /// <summary>
    /// TourProduct SEO route publication: Access-backed <c>seo.tour-posture.write</c>.
    /// </summary>
    public const string SeoTourPostureWrite = "Access.Seo.TourPosture.Write";

    /// <summary>
    /// Pricing Admin reads: Access-backed <c>pricing.prices.read</c>.
    /// </summary>
    public const string PricingPricesRead = "Access.Pricing.Prices.Read";

    /// <summary>
    /// Pricing Admin mutations: Access-backed <c>pricing.prices.write</c>.
    /// </summary>
    public const string PricingPricesWrite = "Access.Pricing.Prices.Write";

    /// <summary>
    /// Agency Marketplace profile reads: Access-backed <c>agency.marketplace.profile.read</c>.
    /// </summary>
    public const string AgencyMarketplaceProfileRead = "Access.AgencyMarketplace.Profile.Read";

    /// <summary>
    /// Agency Marketplace profile mutations: Access-backed <c>agency.marketplace.profile.write</c>.
    /// </summary>
    public const string AgencyMarketplaceProfileWrite = "Access.AgencyMarketplace.Profile.Write";

    /// <summary>
    /// Agency Marketplace offer reads: Access-backed <c>agency.marketplace.offers.read</c>.
    /// </summary>
    public const string AgencyMarketplaceOffersRead = "Access.AgencyMarketplace.Offers.Read";

    /// <summary>
    /// Agency Marketplace offer mutations: Access-backed <c>agency.marketplace.offers.write</c>.
    /// </summary>
    public const string AgencyMarketplaceOffersWrite = "Access.AgencyMarketplace.Offers.Write";
}
