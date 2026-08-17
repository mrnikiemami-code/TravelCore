namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submission-time planning context copied onto a Lead (TC-P18-T002 / P18-R2; preferences P18-R4).
/// </summary>
public sealed class LeadSubmissionSnapshot
{
    private LeadSubmissionSnapshot()
    {
        Preferences = null!;
    }

    private LeadSubmissionSnapshot(
        int capturedPlanningRevision,
        string? capturedPlanningNote,
        TravelPreferenceSnapshot preferences)
    {
        CapturedPlanningRevision = capturedPlanningRevision;
        CapturedPlanningNote = capturedPlanningNote;
        Preferences = preferences;
    }

    public int CapturedPlanningRevision { get; private set; }

    public string? CapturedPlanningNote { get; private set; }

    public TravelPreferenceSnapshot Preferences { get; private set; }

    internal static LeadSubmissionSnapshot CaptureFrom(TripIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        return new LeadSubmissionSnapshot(
            intent.PlanningRevision,
            intent.PlanningNote,
            intent.Preferences.CaptureSnapshot());
    }
}
