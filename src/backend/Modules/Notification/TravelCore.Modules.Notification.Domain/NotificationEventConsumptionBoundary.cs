namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R6 event consumption boundary marker. Downstream async consumer posture without delivery persistence.
/// </summary>
public static class NotificationEventConsumptionBoundary
{
    public const string DownstreamConsumerOwner = "Notification";
    public const string PublisherDoesNotOrchestrateDelivery = "Publishers do not orchestrate delivery";
    public const string CoreCorrectnessIndependentOfDelivery = "Core domain correctness must not depend on delivery success";
    public const string IdempotentDeliveryPosture = "Idempotent delivery posture required";

    public const bool EventConsumptionBoundaryImplemented = true;
    public const bool SemanticEventConsumerPortImplemented = true;
    public const bool DeliveryOrchestrationPersistenceImplemented = false;
    public const bool OutboxConsumerImplemented = false;
    public const bool SynchronousPublisherToProviderCallImplemented = false;
}
