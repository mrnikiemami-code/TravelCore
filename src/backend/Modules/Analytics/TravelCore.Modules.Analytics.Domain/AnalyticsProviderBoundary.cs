namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R3 provider abstraction boundary marker. Port/contracts only — no named production adapter or dispatch persistence in T006.
/// </summary>
public static class AnalyticsProviderBoundary
{
    public const string DispatchOwner = "Analytics";
    public const string PublisherDoesNotCallVendorDirectly =
        "Domain modules must not call Mixpanel/GA/Amplitude/etc. directly";
    public const string ZeroProviderPostureValid = "Zero registered providers is valid until explicit lock";
    public const string ProviderNeutralContractsOnly = "Provider-neutral dispatch contracts only";

    public const bool AnalyticsOwnsProviderAbstraction = true;
    public const bool ProviderPortImplemented = true;
    public const bool NamedProductionAdapterImplemented = false;
    public const bool ProviderExecutionPersistenceImplemented = false;
    public const bool DispatchStatePersistenceImplemented = false;
    public const bool MixpanelClientImplemented = false;
    public const bool GoogleAnalyticsClientImplemented = false;
    public const bool AmplitudeClientImplemented = false;
    public const bool SegmentRuntimeImplemented = false;
    public const bool PublicApiImplemented = false;
}
