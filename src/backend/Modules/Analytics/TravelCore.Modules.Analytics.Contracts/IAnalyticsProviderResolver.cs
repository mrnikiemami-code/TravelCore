namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Resolves a trusted configured AnalyticsProviderKey to a dispatch adapter. No ranking or failover (P27-R3).
/// Zero registered providers is valid until explicit lock.
/// </summary>
public interface IAnalyticsProviderResolver
{
    IAnalyticsDispatchProvider? Resolve(AnalyticsProviderKey providerKey);

    AnalyticsProviderDescriptor? Describe(AnalyticsProviderKey providerKey);

    IReadOnlyList<AnalyticsProviderDescriptor> ListDescriptors();

    AnalyticsProviderCapabilityStatus Check(
        AnalyticsProviderKey providerKey,
        AnalyticsProviderCapability required);
}
