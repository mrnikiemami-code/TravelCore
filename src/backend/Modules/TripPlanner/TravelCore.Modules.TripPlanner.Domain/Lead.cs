using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submitted follow-up request derived from a TripIntent (TC-P18-T002 / P18-R2; contact P18-R3; lifecycle P18-R5; consent P18-R7).
/// Not a Booking, Quote, CRM opportunity, or live alias of TripIntent.
/// </summary>
public sealed class Lead
{
    private Lead()
    {
        Snapshot = null!;
        Contact = null!;
        Consent = null!;
    }

    private Lead(
        LeadId id,
        TripIntentId sourceTripIntentId,
        LeadSubmissionSnapshot snapshot,
        LeadContactSnapshot contact,
        LeadConsentSnapshot consent,
        PlannerActorReference? actorReference,
        Instant submittedAt)
    {
        Id = id;
        SourceTripIntentId = sourceTripIntentId;
        Snapshot = snapshot;
        Contact = contact;
        Consent = consent;
        ActorReference = actorReference;
        Status = LeadStatus.Submitted;
        SubmittedAt = submittedAt;
        CreatedAt = submittedAt;
        StatusChangedAt = submittedAt;
        UpdatedAt = submittedAt;
    }

    public LeadId Id { get; private set; }

    public TripIntentId SourceTripIntentId { get; private set; }

    public PlannerActorReference? ActorReference { get; private set; }

    public LeadStatus Status { get; private set; }

    public LeadSubmissionSnapshot Snapshot { get; private set; }

    public LeadContactSnapshot Contact { get; private set; }

    public LeadConsentSnapshot Consent { get; private set; }

    public Instant SubmittedAt { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static Lead CreateFromTripIntent(
        TripIntent intent,
        Instant submittedAt,
        LeadContactSnapshot contact,
        LeadConsentSnapshot consent,
        PlannerActorReference? actorReference)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(consent);
        if (submittedAt == default)
        {
            throw new ArgumentException("SubmittedAt cannot be default.", nameof(submittedAt));
        }

        consent.ValidateForLeadSubmission(contact);

        return new Lead(
            LeadId.New(),
            intent.Id,
            intent.CaptureSubmissionSnapshot(),
            contact,
            consent.CaptureCopy(),
            actorReference,
            submittedAt);
    }

    internal void ApplyStatusChange(LeadStatus status, Instant now)
    {
        Status = status;
        StatusChangedAt = now;
        UpdatedAt = now;
    }
}
