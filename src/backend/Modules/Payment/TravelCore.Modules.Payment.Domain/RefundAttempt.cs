using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// One concrete provider refund execution attempt owned by a logical Refund (P20-R6).
/// </summary>
public sealed class RefundAttempt
{
    private RefundAttempt()
    {
    }

    internal RefundAttempt(RefundAttemptId id, Instant createdAt)
    {
        Id = id;
        Status = RefundAttemptStatus.Created;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
    }

    public RefundAttemptId Id { get; private set; }

    public RefundAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public ProviderKey? ProviderKey { get; private set; }

    public ProviderRequestReference? ProviderRequestReference { get; private set; }

    public ProviderTransactionReference? ProviderTransactionReference { get; private set; }

    public bool IsTerminal =>
        Status is RefundAttemptStatus.Succeeded or RefundAttemptStatus.Failed;

    public bool IsActive =>
        Status is RefundAttemptStatus.Created or RefundAttemptStatus.Initiated;

    internal void AttachProviderCorrelation(
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        if (ProviderKey is { } existingKey && !existingKey.Equals(providerKey))
        {
            throw new InvalidOperationException("RefundAttempt cannot change ProviderKey.");
        }

        ProviderKey = providerKey;
        ProviderRequestReference ??= requestReference;
        ProviderTransactionReference ??= transactionReference;
    }

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == RefundAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != RefundAttemptStatus.Created)
        {
            throw new InvalidOperationException("Only a Created RefundAttempt can become Initiated.");
        }

        Status = RefundAttemptStatus.Initiated;
        InitiatedAt = now;
        StatusChangedAt = now;
    }

    internal void RecordFailure(Instant now)
    {
        EnsureClock(now);
        if (Status == RefundAttemptStatus.Failed)
        {
            return;
        }

        if (Status == RefundAttemptStatus.Succeeded)
        {
            throw new InvalidOperationException("Succeeded RefundAttempt cannot become Failed.");
        }

        Status = RefundAttemptStatus.Failed;
        StatusChangedAt = now;
    }

    internal void RecordAuthoritativeSuccess(Instant now)
    {
        EnsureClock(now);
        if (Status == RefundAttemptStatus.Succeeded)
        {
            return;
        }

        if (Status == RefundAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed RefundAttempt cannot become Succeeded.");
        }

        Status = RefundAttemptStatus.Succeeded;
        StatusChangedAt = now;
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
