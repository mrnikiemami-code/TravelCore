namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Minimal contact-channel preference for follow-up (P18-R7). Not SMS/WhatsApp provider semantics.
/// </summary>
public enum LeadContactChannelPreference
{
    Email = 1,
    Phone = 2,
}
