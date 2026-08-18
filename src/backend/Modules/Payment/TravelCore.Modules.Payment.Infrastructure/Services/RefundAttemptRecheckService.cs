using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Callable Refund reconciliation. No scheduler. Does not auto-retry (P20-R6).
/// </summary>
internal sealed class RefundAttemptRecheckService
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IClock _clock;

    public RefundAttemptRecheckService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<PaymentVerificationResult?> RecheckAsync(
        RefundAttemptId attemptId,
        CancellationToken cancellationToken = default)
    {
        var refund = await _db.Refunds
            .Include(item => item.Attempts)
            .Include(item => item.Amount)
            .SingleOrDefaultAsync(
                item => item.Attempts.Any(attempt => attempt.Id == attemptId),
                cancellationToken);
        if (refund is null)
        {
            return null;
        }

        var attempt = refund.Attempts.Single(item => item.Id.Equals(attemptId));
        if (attempt.ProviderKey is not { } providerKey)
        {
            _db.RefundReconciliationIssues.Add(
                RefundReconciliationIssue.Create(
                    refund.Id,
                    attempt.Id,
                    RefundReconciliationIssueKind.UnknownProviderTransaction,
                    _clock.GetCurrentInstant()));
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var gateway = _resolver.Resolve(providerKey);
        if (gateway is null)
        {
            return null;
        }

        var result = await gateway.QueryRefundStatusAsync(
            new PaymentVerificationRequest(
                providerKey,
                attempt.ProviderRequestReference,
                attempt.ProviderTransactionReference),
            cancellationToken);

        var now = _clock.GetCurrentInstant();
        var status = VerifiedRefundOutcomeApplier.ApplyVerification(refund, attempt, result, now);
        RecordIssue(refund, attempt, status, now);
        RefundSucceededOutboxWriter.EnqueueIfSucceeded(_db, refund, now, status);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private void RecordIssue(
        Refund refund,
        RefundAttempt attempt,
        VerificationApplyStatus status,
        Instant now)
    {
        if (status == VerificationApplyStatus.Contradiction)
        {
            _db.RefundReconciliationIssues.Add(
                RefundReconciliationIssue.Create(
                    refund.Id,
                    attempt.Id,
                    RefundReconciliationIssueKind.ContradictoryProviderState,
                    now));
        }
        else if (status is VerificationApplyStatus.AmountMismatch or VerificationApplyStatus.CurrencyMismatch)
        {
            _db.RefundReconciliationIssues.Add(
                RefundReconciliationIssue.Create(
                    refund.Id,
                    attempt.Id,
                    status == VerificationApplyStatus.AmountMismatch
                        ? RefundReconciliationIssueKind.AmountMismatch
                        : RefundReconciliationIssueKind.CurrencyMismatch,
                    now));
        }
    }
}
