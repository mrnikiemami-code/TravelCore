namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R4/R6 idempotency and downstream ingestion locks.
/// </summary>
public static class AnalyticsIdempotencyBoundary
{
    public const string FailedDispatchDoesNotRollbackSourceOfRecord =
        "FailedDispatch != SourceOfRecordRollback";
    public const string DuplicateEventIsNotDuplicateSideEffect = "DuplicateEvent != DuplicateSideEffect";
    public const string ExactlyOnceDispatch = "NOT ASSUMED";
    public const string SynchronousDispatchRequiredForCoreCorrectness = "NOT ALLOWED";
    public const bool DownstreamAsyncIngestionPortImplemented = true;
    public const bool IdempotentIngestionPostureDeclared = true;
    public const bool EventPersistenceImplemented = false;
    public const bool OutboxConsumerImplemented = false;
    public const bool AutomaticRetrySchedulerImplemented = false;
}
