namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Provider-declared delivery capabilities. Notification core does not infer these from ProviderKey (P25-R3).
/// </summary>
[Flags]
public enum NotificationProviderCapability
{
    None = 0,
    EmailDelivery = 1,
    SmsDelivery = 2,
    InAppDelivery = 4,
}

/// <summary>
/// Safe provider descriptor. Secrets and vendor account details are excluded (P25-R3).
/// </summary>
public sealed record NotificationProviderDescriptor(
    string ProviderKey,
    string DisplayName,
    NotificationProviderCapability Capabilities,
    bool Enabled);

public static class NotificationProviderCapabilitySet
{
    public const NotificationProviderCapability All =
        NotificationProviderCapability.EmailDelivery
        | NotificationProviderCapability.SmsDelivery
        | NotificationProviderCapability.InAppDelivery;

    public static readonly string[] ExactValues =
    [
        nameof(NotificationProviderCapability.EmailDelivery),
        nameof(NotificationProviderCapability.SmsDelivery),
        nameof(NotificationProviderCapability.InAppDelivery),
    ];
}

public enum NotificationProviderCapabilityStatus
{
    Available = 0,
    UnknownProvider = 1,
    DisabledProvider = 2,
    UnsupportedCapability = 3,
}
