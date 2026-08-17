namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R2: TripIntent and Lead are distinct TripPlanner aggregates.
/// </summary>
public static class TripPlannerAggregateBoundary
{
    public const string TripIntentNotEqualLead = "TripIntent != Lead";
    public const string LeadNotEqualBooking = "Lead != Booking";
    public const string LeadNotEqualQuote = "Lead != Quote";
    public const string LeadNotEqualCrmOpportunity = "Lead != CRM Opportunity";
    public const string TripIntentNotEqualQuote = "TripIntent != Quote";
    public const string SubmissionSnapshotInvariant =
        "Changing TripIntent after Lead creation must not silently change an existing Lead";
}
