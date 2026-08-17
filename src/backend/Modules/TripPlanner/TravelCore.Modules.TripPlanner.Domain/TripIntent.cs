using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Mutable planning intent owned by TripPlanner (TC-P18-T002 / P18-R2; identity P18-R3; preferences P18-R4).
/// </summary>
public sealed class TripIntent
{
    public const int PlanningNoteMaxLength = 500;

    private TripIntent()
    {
        DraftAccessToken = null!;
        Preferences = null!;
    }

    private TripIntent(TripIntentId id, TripIntentDraftAccessToken draftAccessToken, Instant createdAt)
    {
        Id = id;
        DraftAccessToken = draftAccessToken;
        Preferences = TravelPreferences.Empty();
        PlanningRevision = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TripIntentId Id { get; private set; }

    public TripIntentDraftAccessToken DraftAccessToken { get; private set; }

    public PlannerActorReference? ActorReference { get; private set; }

    public int PlanningRevision { get; private set; }

    /// <summary>Supplementary note; structured preferences remain primary (P18-R4).</summary>
    public string? PlanningNote { get; private set; }

    public TravelPreferences Preferences { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static TripIntent Create(Instant now, string? planningNote = null)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        var intent = new TripIntent(TripIntentId.New(), TripIntentDraftAccessToken.Generate(), now);
        intent.PlanningNote = NormalizePlanningNote(planningNote);
        return intent;
    }

    public void AssociateActor(PlannerActorReference actorReference, Instant now)
    {
        if (actorReference.ActorId == Guid.Empty)
        {
            throw new ArgumentException("Actor id cannot be empty.", nameof(actorReference));
        }

        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        ActorReference = actorReference;
        Touch(now);
    }

    public void UpdatePlanningNote(string? planningNote, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        PlanningNote = NormalizePlanningNote(planningNote);
        Touch(now);
    }

    public void UpdatePreferences(Action<TravelPreferences> configure, Instant now)
    {
        ArgumentNullException.ThrowIfNull(configure);
        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        configure(Preferences);
        Touch(now);
    }

    public Lead SubmitAsLead(
        Instant submittedAt,
        LeadContactSnapshot? contact = null,
        LeadConsentSnapshot? consent = null)
        => TripIntentLeadSubmissionBoundary.Submit(this, submittedAt, contact, consent);

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

    internal LeadSubmissionSnapshot CaptureSubmissionSnapshot() =>
        LeadSubmissionSnapshot.CaptureFrom(this);

    private void Touch(Instant now)
    {
        PlanningRevision++;
        UpdatedAt = now;
    }
}
