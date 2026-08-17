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
    /// TourProduct SEO route publication: Access-backed <c>seo.tour-posture.write</c>.
    /// </summary>
    public const string SeoTourPostureWrite = "Access.Seo.TourPosture.Write";
}
