namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Provider-neutral delivery attempt outcome. No fake success posture (P25-R3).
/// </summary>
public enum NotificationDeliveryOutcome
{
    Unknown = 0,
    Accepted = 1,
    Failed = 2,
}

/// <summary>
/// Neutral delivery request. Template rendering and persistence remain deferred beyond T006.
/// </summary>
public sealed record NotificationDeliveryRequest(
    NotificationProviderKey ProviderKey,
    string ChannelKind,
    string RecipientReference,
    string ContentReference);

/// <summary>
/// Provider-neutral delivery result. External references are opaque correlation only.
/// </summary>
public sealed record NotificationDeliveryResult(
    NotificationDeliveryOutcome Outcome,
    NotificationProviderKey ProviderKey,
    string? ExternalReference = null);
