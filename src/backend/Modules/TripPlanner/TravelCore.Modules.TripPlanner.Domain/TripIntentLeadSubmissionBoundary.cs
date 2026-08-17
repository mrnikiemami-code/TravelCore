using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Domain submission boundary: TripIntent -> Lead (TC-P18-T002 / P18-R2; contact/actor P18-R3; consent P18-R7).
/// </summary>
public static class TripIntentLeadSubmissionBoundary
{
    public static Lead Submit(
        TripIntent intent,
        Instant submittedAt,
        LeadContactSnapshot? contact = null,
        LeadConsentSnapshot? consent = null,
        PlannerActorReference? actorReference = null)
    {
        ArgumentNullException.ThrowIfNull(intent);
        intent.Preferences.ValidateForLeadSubmission();
        var resolvedContact = contact ?? LeadContactSnapshot.Empty;
        var resolvedConsent = consent ?? LeadConsentSnapshot.InferForSubmission(resolvedContact, submittedAt);
        var resolvedActor = actorReference ?? intent.ActorReference;
        return Lead.CreateFromTripIntent(intent, submittedAt, resolvedContact, resolvedConsent, resolvedActor);
    }
}
