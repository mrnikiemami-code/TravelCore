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
        ("seo.destination-posture.write", "Publish Destination SEO routes and set IndexPolicy posture"),
        ("media.assets.write", "Upload and mutate MediaAsset technical metadata"),
        ("place.places.write", "Create/update Place catalog entries and owned fields"),
        ("seo.place-posture.write", "Publish Place SEO routes (IndexPolicy remains explicit / default noindex)"),
        ("content.items.write", "Create/update ContentItem editorial entries and owned fields"),
        ("seo.content-posture.write", "Publish Content SEO routes (IndexPolicy remains explicit / default noindex)"),
        ("tour.products.write", "Create/update TourProduct catalog entries and owned fields"),
        ("tour.departures.read", "Read TourDeparture execution catalog"),
        ("tour.departures.write", "Create/update TourDeparture execution fields"),
        ("seo.tour-posture.write", "Publish TourProduct SEO routes (IndexPolicy remains explicit / default noindex)"),
        ("pricing.prices.read", "Read Pricing admin prices"),
        ("pricing.prices.write", "Create/update Pricing admin prices, components, and occupancy rules"),
        ("agency.marketplace.profile.read", "Read Agency Marketplace profiles"),
        ("agency.marketplace.profile.write", "Create/update Agency Marketplace profiles"),
        ("agency.marketplace.offers.read", "Read Agency Marketplace offers"),
        ("agency.marketplace.offers.write", "Create/update Agency Marketplace offers"),
        ("agency.marketplace.offers.moderate", "Approve or reject Agency Marketplace offer publication")
    ];

    public const string AdminRoleCode = "admin";
    public const string AdminRoleDisplayName = "Administrator";

    public const string AgencyRoleCode = "agency";
    public const string AgencyRoleDisplayName = "Agency operator";

    public static IReadOnlyList<(string Code, string DisplayName)> AgencyPresentationBaseline { get; } =
    [
        ("agency.panel.open", "Open Agency presentation panel"),
        ("agency.marketplace.profile.read", "Read Agency Marketplace profiles"),
        ("agency.marketplace.profile.write", "Create/update Agency Marketplace profiles"),
        ("agency.marketplace.offers.read", "Read Agency Marketplace offers"),
        ("agency.marketplace.offers.write", "Create/update Agency Marketplace offers")
    ];
}
