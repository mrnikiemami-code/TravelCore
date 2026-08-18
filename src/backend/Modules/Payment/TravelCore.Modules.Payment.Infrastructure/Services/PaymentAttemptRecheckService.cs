using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Callable Payment reconciliation. No scheduler. Does not auto-retry (P20-R4).
/// </summary>
internal sealed class PaymentAttemptRecheckService
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IClock _clock;

    public PaymentAttemptRecheckService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<PaymentVerificationResult?> RecheckAsync(
        PaymentAttemptId attemptId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(item => item.Attempts)
            .Include(item => item.ExecutionSnapshot)
            .SingleOrDefaultAsync(
                item => item.Attempts.Any(attempt => attempt.Id == attemptId),
                cancellationToken);
        if (payment is null)
        {
            return null;
        }

        var attempt = payment.Attempts.Single(item => item.Id.Equals(attemptId));
        if (attempt.ProviderKey is not { } providerKey)
        {
            _db.ReconciliationIssues.Add(
                PaymentReconciliationIssue.Create(
                    payment.Id,
                    attempt.Id,
                    PaymentReconciliationIssueKind.UnknownProviderTransaction,
                    _clock.GetCurrentInstant()));
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var gateway = _resolver.Resolve(providerKey);
        if (gateway is null)
        {
            return null;
        }

        var result = await gateway.QueryPaymentStatusAsync(
            new PaymentVerificationRequest(
                providerKey,
                attempt.ProviderRequestReference,
                attempt.ProviderTransactionReference),
            cancellationToken);

        var now = _clock.GetCurrentInstant();
        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            result,
            now);
        if (status == VerificationApplyStatus.Contradiction)
        {
            _db.ReconciliationIssues.Add(
                PaymentReconciliationIssue.Create(
                    payment.Id,
                    attempt.Id,
                    PaymentReconciliationIssueKind.ContradictoryProviderState,
                    now));
        }
        else if (status is VerificationApplyStatus.AmountMismatch or VerificationApplyStatus.CurrencyMismatch)
        {
            _db.ReconciliationIssues.Add(
                PaymentReconciliationIssue.Create(
                    payment.Id,
                    attempt.Id,
                    status == VerificationApplyStatus.AmountMismatch
                        ? PaymentReconciliationIssueKind.AmountMismatch
                        : PaymentReconciliationIssueKind.CurrencyMismatch,
                    now));
        }

        PaymentSuccessOutboxWriter.EnqueueIfSucceeded(_db, payment, now, status);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
