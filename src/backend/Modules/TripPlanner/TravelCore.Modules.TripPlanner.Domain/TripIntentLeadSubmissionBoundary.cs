using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Domain submission boundary: TripIntent -> Lead (TC-P18-T002 / P18-R2).
/// </summary>
public static class TripIntentLeadSubmissionBoundary
{
    public static Lead Submit(TripIntent intent, Instant submittedAt)
    {
        ArgumentNullException.ThrowIfNull(intent);
        return Lead.CreateFromTripIntent(intent, submittedAt);
    }
}
