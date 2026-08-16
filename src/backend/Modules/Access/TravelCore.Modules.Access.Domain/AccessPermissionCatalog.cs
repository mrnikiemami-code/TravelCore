namespace TravelCore.Modules.Access.Domain;

/// <summary>
/// Explicit minimal Admin permission catalog for P03 baseline seed (T005).
/// </summary>
public static class AccessPermissionCatalog
{
    public static IReadOnlyList<(string Code, string DisplayName)> AdminBaseline { get; } =
    [
        ("identity.accounts.read", "Read identity accounts"),
        ("identity.accounts.write", "Create/update identity accounts"),
        ("party.parties.read", "Read parties"),
        ("party.parties.write", "Create/update parties"),
        ("access.permissions.read", "Read access permissions"),
        ("access.permissions.write", "Manage access permissions"),
        ("access.roles.read", "Read access roles"),
        ("access.roles.write", "Manage access roles"),
        ("destination.destinations.write", "Create/update Destination hierarchy and owned fields"),
        ("seo.destination-posture.write", "Publish Destination SEO routes and set IndexPolicy posture")
    ];

    public const string AdminRoleCode = "admin";
    public const string AdminRoleDisplayName = "Administrator";

    public const string AgencyRoleCode = "agency";
    public const string AgencyRoleDisplayName = "Agency operator";

    public static IReadOnlyList<(string Code, string DisplayName)> AgencyPresentationBaseline { get; } =
    [
        ("agency.panel.open", "Open Agency presentation panel")
    ];
}
