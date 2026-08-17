using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submitted follow-up request derived from a TripIntent (TC-P18-T002 / P18-R2).
/// Not a Booking, Quote, CRM opportunity, or live alias of TripIntent.
/// </summary>
public sealed class Lead
{
    private Lead()
    {
        Snapshot = null!;
    }

    private Lead(
        LeadId id,
        TripIntentId sourceTripIntentId,
        LeadSubmissionSnapshot snapshot,
        Instant submittedAt)
    {
        Id = id;
        SourceTripIntentId = sourceTripIntentId;
        Snapshot = snapshot;
        Status = LeadStatus.Submitted;
        SubmittedAt = submittedAt;
        CreatedAt = submittedAt;
    }

    public LeadId Id { get; private set; }

    public TripIntentId SourceTripIntentId { get; private set; }

    public LeadStatus Status { get; private set; }

    public LeadSubmissionSnapshot Snapshot { get; private set; }

    public Instant SubmittedAt { get; private set; }

    public Instant CreatedAt { get; private set; }

    internal static Lead CreateFromTripIntent(TripIntent intent, Instant submittedAt)
    {
        ArgumentNullException.ThrowIfNull(intent);
        if (submittedAt == default)
        {
            throw new ArgumentException("SubmittedAt cannot be default.", nameof(submittedAt));
        }

        return new Lead(
            LeadId.New(),
            intent.Id,
            intent.CaptureSubmissionSnapshot(),
            submittedAt);
    }
}
