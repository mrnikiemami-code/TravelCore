namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R7: TripPlanner consent/privacy boundaries vs marketing automation, CRM, and Notification delivery.
/// </summary>
public static class TripPlannerConsentBoundary
{
    public const string ContactPermissionNotEqualMarketingConsent = "ContactPermission != MarketingConsent";
    public const string ConsentNotEqualNotificationDelivery = "Consent != NotificationDelivery";
    public const string LeadContactSnapshotNotEqualLeadConsentSnapshot = "LeadContactSnapshot != LeadConsentSnapshot";
    public const string FollowUpContactAllowedNotEqualAgencyDataSharingPermission =
        "FollowUpContactAllowed != AgencyDataSharingPermission";
    public const string PlannerConsentNotEqualBookingAcceptance = "Planner Consent != Booking Acceptance";
    public const string RetentionPolicyRequiresFutureConfiguration =
        "RetentionPolicy = future explicit operational/legal configuration";
    public const string NotificationProviderImplementationDeferred =
        "Notification provider implementation = DEFERRED";
    public const bool ConsentModelImplemented = true;
    public const bool MarketingConsentRequiredForLeadSubmission = false;
    public const bool NotificationProviderImplemented = false;
}
