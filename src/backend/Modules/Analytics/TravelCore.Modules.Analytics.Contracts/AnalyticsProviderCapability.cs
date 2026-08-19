namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Provider-declared analytics dispatch capabilities. Analytics core does not infer these from ProviderKey (P27-R3).
/// </summary>
[Flags]
public enum AnalyticsProviderCapability
{
    None = 0,
    EventDispatch = 1,
    BatchExport = 2,
}

/// <summary>
/// Safe provider descriptor. Secrets and vendor account details are excluded (P27-R3).
/// </summary>
public sealed record AnalyticsProviderDescriptor(
    string ProviderKey,
    string DisplayName,
    AnalyticsProviderCapability Capabilities,
    bool Enabled);

public static class AnalyticsProviderCapabilitySet
{
    public const AnalyticsProviderCapability All =
        AnalyticsProviderCapability.EventDispatch
        | AnalyticsProviderCapability.BatchExport;

    public static readonly string[] ExactValues =
    [
        nameof(AnalyticsProviderCapability.EventDispatch),
        nameof(AnalyticsProviderCapability.BatchExport),
    ];
}

public enum AnalyticsProviderCapabilityStatus
{
    Available = 0,
    UnknownProvider = 1,
    DisabledProvider = 2,
    UnsupportedCapability = 3,
}
