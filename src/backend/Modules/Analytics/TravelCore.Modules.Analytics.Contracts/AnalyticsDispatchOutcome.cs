namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Provider-neutral dispatch attempt outcome. No fake success posture (P27-R3).
/// </summary>
public enum AnalyticsDispatchOutcome
{
    Unknown = 0,
    Accepted = 1,
    Failed = 2,
}

/// <summary>
/// Neutral analytics dispatch request. Warehouse persistence and vendor SDK execution remain deferred beyond T006.
/// </summary>
public sealed record AnalyticsDispatchRequest(
    AnalyticsProviderKey ProviderKey,
    AnalyticsProductEventKind EventKind,
    string SourceModule,
    string CorrelationReference,
    string ResourceReference,
    string OccurredAtReference);

/// <summary>
/// Provider-neutral dispatch result. External references are opaque correlation only.
/// </summary>
public sealed record AnalyticsDispatchResult(
    AnalyticsDispatchOutcome Outcome,
    AnalyticsProviderKey ProviderKey,
    string? ExternalReference = null);
