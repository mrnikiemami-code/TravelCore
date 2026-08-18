using NodaTime;
using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Payment-owned refund discrepancy. Not a failed logical Refund (P20-R6).
/// </summary>
public sealed class RefundReconciliationIssue
{
    private RefundReconciliationIssue()
    {
    }

    private RefundReconciliationIssue(
        Guid id,
        RefundId refundId,
        RefundAttemptId attemptId,
        RefundReconciliationIssueKind kind,
        Instant detectedAt)
    {
        Id = id;
        RefundId = refundId;
        AttemptId = attemptId;
        Kind = kind;
        DetectedAt = detectedAt;
    }

    public Guid Id { get; private set; }

    public RefundId RefundId { get; private set; }

    public RefundAttemptId AttemptId { get; private set; }

    public RefundReconciliationIssueKind Kind { get; private set; }

    public Instant DetectedAt { get; private set; }

    public Instant? ResolvedAt { get; private set; }

    public static RefundReconciliationIssue Create(
        RefundId refundId,
        RefundAttemptId attemptId,
        RefundReconciliationIssueKind kind,
        Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("DetectedAt cannot be default.", nameof(now));
        }

        return new RefundReconciliationIssue(Uuid7.New(), refundId, attemptId, kind, now);
    }
}
