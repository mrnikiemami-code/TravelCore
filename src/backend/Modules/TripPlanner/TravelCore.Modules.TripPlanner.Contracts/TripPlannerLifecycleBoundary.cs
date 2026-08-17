namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R5: TripPlanner Lead lifecycle vs CRM, Booking, Pricing, agency routing.
/// </summary>
public static class TripPlannerLifecycleBoundary
{
    public const string LeadStatusNotEqualCrmPipelineStage = "LeadStatus != CRM Pipeline Stage";
    public const string LeadNotEqualCrmOpportunity = "Lead != CRM Opportunity";
    public const string LeadStatusNotEqualBookingStatus = "LeadStatus != BookingStatus";
    public const string LeadStatusNotEqualQuoteStatus = "LeadStatus != QuoteStatus";
    public const string ClosedNotEqualBookingConversion = "Closed != Booking conversion";
    public const string ContactedNotEqualQualification = "Contacted != Qualification";
    public const string LeadStatusChangeNotEqualTripIntentChange = "LeadStatus change != TripIntent change";
    public const string FullQualificationDeferred = "Full qualification = DEFERRED";
    public const bool LeadLifecycleImplemented = true;
}
