namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Downstream semantic event ingestion port. Async/idempotent posture; no rollback of committed SoR (P27-R6).
/// </summary>
public interface IAnalyticsSemanticEventConsumer
{
    Task<AnalyticsEventIngestionOutcome> IngestAsync(
        AnalyticsSemanticEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Ingestion outcome without fabricating dispatch success (P27-R6).
/// </summary>
public enum AnalyticsEventIngestionOutcome
{
    Ignored = 0,
    AcceptedForDispatch = 1,
    DuplicateIgnored = 2,
    Failed = 3,
}
