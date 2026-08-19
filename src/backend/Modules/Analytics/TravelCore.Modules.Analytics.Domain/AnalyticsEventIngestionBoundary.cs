namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R6 event ingestion boundary marker. Downstream async ingestion posture without event persistence.
/// </summary>
public static class AnalyticsEventIngestionBoundary
{
    public const string DownstreamIngestionOwner = "Analytics";
    public const string PublisherDoesNotOrchestrateDispatch = "Publishers do not orchestrate analytics dispatch";
    public const string CoreCorrectnessIndependentOfDispatch =
        "Core domain correctness must not depend on analytics dispatch success";
    public const string IdempotentIngestionPosture = "Idempotent ingestion posture required";
    public const string NonBlockingDispatchPosture = "Dispatch is downstream and non-blocking for domain commits";

    public const bool EventIngestionBoundaryImplemented = true;
    public const bool SemanticEventConsumerPortImplemented = true;
    public const bool IngestionPersistenceImplemented = false;
    public const bool OutboxConsumerImplemented = false;
    public const bool SynchronousPublisherToProviderCallImplemented = false;
    public const bool ProviderExecutionImplemented = false;
}
