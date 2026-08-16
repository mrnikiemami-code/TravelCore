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
}
