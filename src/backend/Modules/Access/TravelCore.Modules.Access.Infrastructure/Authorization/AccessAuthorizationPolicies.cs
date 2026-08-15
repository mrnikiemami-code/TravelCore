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
}
