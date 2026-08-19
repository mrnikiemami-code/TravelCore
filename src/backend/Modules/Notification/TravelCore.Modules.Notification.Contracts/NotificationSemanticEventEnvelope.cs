namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Minimal semantic event kinds consumed downstream by Notification (P25-R6). Not business workflow ownership.
/// </summary>
public enum NotificationSemanticEventKind
{
    Unknown = 0,
    LeadSubmitted = 1,
    BookingConfirmed = 2,
    PaymentSucceeded = 3,
}

/// <summary>
/// Neutral semantic event envelope from publisher modules. Notification consumes facts/intent only (P25-R6).
/// </summary>
public sealed record NotificationSemanticEventEnvelope(
    NotificationSemanticEventKind EventKind,
    string SourceModule,
    string CorrelationReference,
    string PayloadReference,
    string OccurredAtReference);
