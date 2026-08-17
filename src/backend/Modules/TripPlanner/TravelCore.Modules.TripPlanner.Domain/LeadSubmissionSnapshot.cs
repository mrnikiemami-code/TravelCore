namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submission-time planning context copied onto a Lead (TC-P18-T002 / P18-R2).
/// Neutral placeholder until P18-R4 defines the full preference model.
/// </summary>
public sealed class LeadSubmissionSnapshot
{
    private LeadSubmissionSnapshot()
    {
    }

    private LeadSubmissionSnapshot(int capturedPlanningRevision, string? capturedPlanningNote)
    {
        CapturedPlanningRevision = capturedPlanningRevision;
        CapturedPlanningNote = capturedPlanningNote;
    }

    public int CapturedPlanningRevision { get; private set; }

    public string? CapturedPlanningNote { get; private set; }

    internal static LeadSubmissionSnapshot CaptureFrom(TripIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        return new LeadSubmissionSnapshot(intent.PlanningRevision, intent.PlanningNote);
    }
}
