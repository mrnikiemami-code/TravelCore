namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P18-R8: Public Trip Planner route is presentation/composition only.
/// TripPlanner remains Lead/TripIntent SoT. Search remains retrieval owner. SEO owns IndexPolicy.
/// Honest CTA — no fake Book Now / Checkout / Pay.
/// </summary>
public static class PublicExperienceTripPlannerCompositionBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "TripPlanner";
    public const string IndexPolicyOwner = "Seo";
    public const string FutureRetrievalOwner = "Search";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string PublicRoutePattern = "/plan";
    public const bool CopyPlannerFactsIntoPresentationAllowed = false;
    public const bool PublicExperienceOwnsLeadFacts = false;
    public const bool PublicExperienceOwnsTripIntentFacts = false;
    public const bool SearchEngineAllowed = false;
    public const bool BookingCtaAllowed = false;
    public const bool CheckoutCtaAllowed = false;
    public const bool PaymentCtaAllowed = false;
    public const bool CrmWorkflowAllowed = false;
    public const bool AgencyRoutingAllowed = false;
    public const bool NotificationProviderAllowed = false;
    public const bool PricingQuoteDisplayAllowed = false;
    public const string HonestCtaPosture = "RequestFollowUpOnly";
}
