using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Payment-owned operational discrepancy. Not settlement, accounting, or an ops ticket (P20-R4).
/// </summary>
public sealed class PaymentReconciliationIssue
{
    private PaymentReconciliationIssue()
    {
    }

    private PaymentReconciliationIssue(
        Guid id,
        PaymentId paymentId,
        PaymentAttemptId attemptId,
        PaymentReconciliationIssueKind kind,
        Instant detectedAt)
    {
        Id = id;
        PaymentId = paymentId;
        AttemptId = attemptId;
        Kind = kind;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public PaymentId PaymentId { get; private set; }

    public PaymentAttemptId AttemptId { get; private set; }

    public PaymentReconciliationIssueKind Kind { get; private set; }

    public Instant DetectedAt { get; private set; }

    public Instant? ResolvedAt { get; private set; }

    public static PaymentReconciliationIssue Create(
        PaymentId paymentId,
        PaymentAttemptId attemptId,
        PaymentReconciliationIssueKind kind,
        Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("DetectedAt cannot be default.", nameof(now));
        }

        return new PaymentReconciliationIssue(Uuid7.New(), paymentId, attemptId, kind, now);
    }
}
