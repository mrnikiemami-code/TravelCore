namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R8: TripPlanner owns TripIntent/Lead facts. PublicExperience composes the public planner route.
/// Not Search, Booking, Payment, CRM, agency routing, or Notification delivery.
/// </summary>
public static class TripPlannerPublicCompositionBoundary
{
    public const string FactOwner = "TripPlanner";
    public const string PresentationOwner = "PublicExperience";
    public const string PublicRoutePattern = "/plan";
    public const string PublicApiGroup = "/api/trip-planner/public";
    public const string DraftTokenHeader = "X-TripPlanner-Draft-Token";
    public const bool PublicExperienceOwnsLeadFacts = false;
    public const bool SearchEngineAllowed = false;
    public const bool BookingCtaAllowed = false;
    public const bool PaymentCtaAllowed = false;
    public const bool CheckoutCtaAllowed = false;
    public const bool CrmWorkflowAllowed = false;
    public const bool AgencyRoutingAllowed = false;
    public const bool NotificationProviderAllowed = false;
    public const bool PricingQuoteDisplayAllowed = false;
    public const string HonestCtaPosture = "RequestFollowUpOnly";
}
