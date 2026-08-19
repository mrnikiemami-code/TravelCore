namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Downstream semantic event consumption port. Async/idempotent posture; no rollback of committed SoR (P25-R6).
/// </summary>
public interface INotificationSemanticEventConsumer
{
    Task<NotificationEventConsumptionOutcome> ConsumeAsync(
        NotificationSemanticEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Consumption outcome without fabricating delivery success (P25-R6).
/// </summary>
public enum NotificationEventConsumptionOutcome
{
    Ignored = 0,
    AcceptedForOrchestration = 1,
    DuplicateIgnored = 2,
    Failed = 3,
}
