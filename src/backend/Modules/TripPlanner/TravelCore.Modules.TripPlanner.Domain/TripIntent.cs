using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Mutable planning intent owned by TripPlanner (TC-P18-T002 / P18-R2).
/// Not a Lead, Booking, Quote, CRM opportunity, or Party identity record.
/// </summary>
public sealed class TripIntent
{
    public const int PlanningNoteMaxLength = 500;

    private TripIntent()
    {
    }

    private TripIntent(TripIntentId id, Instant createdAt)
    {
        Id = id;
        PlanningRevision = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TripIntentId Id { get; private set; }

    /// <summary>
    /// Increments when mutable planning context changes. Used for submission snapshots.
    /// </summary>
    public int PlanningRevision { get; private set; }

    /// <summary>
    /// Neutral mutable placeholder until P18-R4 preference model is locked.
    /// </summary>
    public string? PlanningNote { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static TripIntent Create(Instant now, string? planningNote = null)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        var intent = new TripIntent(TripIntentId.New(), now);
        intent.PlanningNote = NormalizePlanningNote(planningNote);
        return intent;
    }

    public void UpdatePlanningNote(string? planningNote, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        PlanningNote = NormalizePlanningNote(planningNote);
        PlanningRevision++;
        UpdatedAt = now;
    }

    public Lead SubmitAsLead(Instant now) => TripIntentLeadSubmissionBoundary.Submit(this, now);

    internal static string? NormalizePlanningNote(string? planningNote)
    {
        if (planningNote is null)
        {
            return null;
        }

        var trimmed = planningNote.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > PlanningNoteMaxLength)
        {
            throw new ArgumentException(
                $"Planning note max length is {PlanningNoteMaxLength}.",
                nameof(planningNote));
        }

        return trimmed;
    }

    internal LeadSubmissionSnapshot CaptureSubmissionSnapshot() => LeadSubmissionSnapshot.CaptureFrom(this);
}
