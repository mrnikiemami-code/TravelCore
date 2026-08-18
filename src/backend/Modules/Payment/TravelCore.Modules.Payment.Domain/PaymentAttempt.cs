using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// One concrete execution attempt owned by a logical Payment (P20-R2).
/// Provider-neutral correlation references are optional until trusted initiation (P20-R3).
/// Not a second Payment obligation. Not a provider transaction entity.
/// </summary>
public sealed class PaymentAttempt
{
    private PaymentAttempt()
    {
    }

    internal PaymentAttempt(PaymentAttemptId id, Instant createdAt)
    {
        Id = id;
        Status = PaymentAttemptStatus.Created;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
    }

    public PaymentAttemptId Id { get; private set; }

    public PaymentAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public ProviderKey? ProviderKey { get; private set; }

    public ProviderRequestReference? ProviderRequestReference { get; private set; }

    public ProviderTransactionReference? ProviderTransactionReference { get; private set; }

    public bool IsTerminal =>
        Status is PaymentAttemptStatus.Succeeded or PaymentAttemptStatus.Failed;

    public bool IsActive =>
        Status is PaymentAttemptStatus.Created or PaymentAttemptStatus.Initiated;

    internal void AttachProviderCorrelation(
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        if (ProviderKey is { } existingKey && !existingKey.Equals(providerKey))
        {
            throw new InvalidOperationException("PaymentAttempt cannot change ProviderKey.");
        }

        if (ProviderRequestReference is { } existingRequest
            && requestReference is { } nextRequest
            && !existingRequest.Equals(nextRequest))
        {
            throw new InvalidOperationException("PaymentAttempt cannot change ProviderRequestReference.");
        }

        if (ProviderTransactionReference is { } existingTransaction
            && transactionReference is { } nextTransaction
            && !existingTransaction.Equals(nextTransaction))
        {
            throw new InvalidOperationException("PaymentAttempt cannot change ProviderTransactionReference.");
        }

        ProviderKey = providerKey;
        ProviderRequestReference ??= requestReference;
        ProviderTransactionReference ??= transactionReference;
    }

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        EnsureNotTerminal();
        if (Status == PaymentAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != PaymentAttemptStatus.Created)
        {
            throw new InvalidOperationException("Only a Created PaymentAttempt can become Initiated.");
        }

        Status = PaymentAttemptStatus.Initiated;
        InitiatedAt = now;
        StatusChangedAt = now;
    }

    internal void RecordFailure(Instant now)
    {
        EnsureClock(now);
        if (Status == PaymentAttemptStatus.Failed)
        {
            return;
        }

        if (Status == PaymentAttemptStatus.Succeeded)
        {
            throw new InvalidOperationException("Succeeded PaymentAttempt cannot become Failed.");
        }

        Status = PaymentAttemptStatus.Failed;
        StatusChangedAt = now;
    }

    internal void RecordAuthoritativeSuccess(Instant now)
    {
        EnsureClock(now);
        if (Status == PaymentAttemptStatus.Succeeded)
        {
            return;
        }

        if (Status == PaymentAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed PaymentAttempt cannot become Succeeded.");
        }

        Status = PaymentAttemptStatus.Succeeded;
        StatusChangedAt = now;
    }

    private void EnsureNotTerminal()
    {
        if (IsTerminal)
        {
            throw new InvalidOperationException("Terminal PaymentAttempt cannot be reopened.");
        }
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
