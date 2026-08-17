namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Minimal TripPlanner-owned Lead lifecycle (TC-P18-T005 / P18-R5).
/// Not CRM pipeline stage, BookingStatus, or QuoteStatus.
/// </summary>
public enum LeadStatus
{
    Submitted = 1,
    Contacted = 2,
    Closed = 3,
    Cancelled = 4,
}
