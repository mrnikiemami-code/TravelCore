namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R3: Domain change reaches Search via outbox + async projection — not in the domain transaction.
/// </summary>
public static class SearchProjectionSyncBoundary
{
    public const string SyncPosture = "TransactionalOutboxPlusAsyncProjectionWorker";
    public const bool DomainTransactionIncludesSearchWrite = false;
    public const bool SearchFailureFailsDomainTransaction = false;
    public const bool ProjectionMustBeRetryable = true;
    public const bool ProjectionMustBeIdempotent = true;
    public const bool RealQueueInfrastructureAllowed = false;
    public const bool RabbitMqDependencyAllowed = false;
    public const bool RankingEngineAllowed = false;
    public const bool FacetingEngineAllowed = false;
    public const bool EmbeddingAllowed = false;
}
