namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R4: Shared Detail Shell + kind-specific section composition.
/// Not independent Experience/Package pages. Not a giant union ViewModel.
/// Package specialized sections are future contributors — not implemented here.
/// </summary>
public static class PublicExperienceDetailComposition
{
    public const string ShellPosture = "SharedShellPlusKindSpecificSections";
    public const string SharedShellOwner = "PublicExperience";
    public const string CatalogFactOwner = "Tour";

    public const string SharedSections =
        "CoverMedia+TitleSummary+Destinations+DepartureSummary+PriceSummary+Policies+OfferReadinessSlot+StickyActions";

    public const string ExperienceSections =
        "Itinerary+Difficulty+Eligibility+Equipment+Meals+Guide+AccommodationPlan+LocalTransport";

    public const string FuturePackageSections = "Flight+Hotel+Transfers+Visa+PackageItinerary";

    public const bool IndependentKindPagesAllowed = false;
    public const bool GiantUnionViewModelAllowed = false;
}
