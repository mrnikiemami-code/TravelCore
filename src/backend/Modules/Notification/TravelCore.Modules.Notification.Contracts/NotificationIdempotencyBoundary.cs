namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// P25-R6 idempotency and downstream consumption locks.
/// </summary>
public static class NotificationIdempotencyBoundary
{
    public const string FailedDeliveryDoesNotRollbackSourceOfRecord = "FailedDelivery != SourceOfRecordRollback";
    public const string DuplicateEventIsNotDuplicateDelivery = "DuplicateEvent != DuplicateDelivery";
    public const string ExactlyOnceDelivery = "NOT ASSUMED";
    public const string SynchronousDeliveryRequiredForCoreCorrectness = "NOT ALLOWED";
    public const bool DownstreamAsyncConsumerPortImplemented = true;
    public const bool IdempotentDeliveryPostureDeclared = true;
    public const bool DeliveryStatePersistenceImplemented = false;
    public const bool OutboxConsumerImplemented = false;
    public const bool AutomaticRetrySchedulerImplemented = false;
}
