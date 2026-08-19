namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Provider-neutral Analytics dispatch port. Vendor-specific types stay in adapters (P27-R3).
/// </summary>
public interface IAnalyticsDispatchProvider
{
    AnalyticsProviderKey Key { get; }

    AnalyticsProviderCapability Capabilities { get; }

    Task<AnalyticsDispatchResult> DispatchAsync(
        AnalyticsDispatchRequest request,
        CancellationToken cancellationToken = default);
}
