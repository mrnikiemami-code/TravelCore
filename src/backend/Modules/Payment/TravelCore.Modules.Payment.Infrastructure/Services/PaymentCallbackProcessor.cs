using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

internal enum PaymentCallbackProcessStatus
{
    Applied = 1,
    Unverified = 2,
    UnknownProvider = 3,
    UnknownAttempt = 4,
    Ignored = 5,
}

internal sealed record PaymentCallbackProcessResult(PaymentCallbackProcessStatus Status);

/// <summary>
/// Trusted callback processor. Unverified envelopes cannot mark success (P20-R3).
/// </summary>
internal sealed class PaymentCallbackProcessor
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IClock _clock;

    public PaymentCallbackProcessor(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<PaymentCallbackProcessResult> ProcessAsync(
        PaymentCallbackEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var gateway = _resolver.Resolve(envelope.ProviderKey);
        if (gateway is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.UnknownProvider);
        }

        var verification = await gateway.VerifyCallbackAsync(envelope, cancellationToken);
        if (!verification.IsVerified || verification.Result is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.Unverified);
        }

        var now = _clock.GetCurrentInstant();
        if (IsRefundCallback(envelope))
        {
            return await ProcessRefundAsync(verification.Result, now, cancellationToken);
        }

        var payment = await FindCorrelatedPaymentAsync(verification.Result, cancellationToken);
        if (payment is null)
        {
            return await ProcessRefundAsync(verification.Result, now, cancellationToken);
        }

        var attempt = FindCorrelatedAttempt(payment, verification.Result);
        if (attempt is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.UnknownAttempt);
        }

        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            verification.Result,
            now);
        if (status is VerificationApplyStatus.AmountMismatch or VerificationApplyStatus.CurrencyMismatch)
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
        return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.Applied);
    }

    private async Task<PaymentCallbackProcessResult> ProcessRefundAsync(
        PaymentVerificationResult result,
        Instant now,
        CancellationToken cancellationToken)
    {
        var refund = await FindCorrelatedRefundAsync(result, cancellationToken);
        if (refund is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.UnknownAttempt);
        }

        var attempt = FindCorrelatedRefundAttempt(refund, result);
        if (attempt is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.UnknownAttempt);
        }

        var status = VerifiedRefundOutcomeApplier.ApplyVerification(refund, attempt, result, now);
        if (status is VerificationApplyStatus.AmountMismatch or VerificationApplyStatus.CurrencyMismatch)
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

        RefundSucceededOutboxWriter.EnqueueIfSucceeded(_db, refund, now, status);
        await _db.SaveChangesAsync(cancellationToken);
        return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.Applied);
    }

    private static bool IsRefundCallback(PaymentCallbackEnvelope envelope)
    {
        return envelope.Headers.TryGetValue(PaymentCallbackKinds.HeaderName, out var kind)
            && string.Equals(kind, PaymentCallbackKinds.Refund, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<PaymentAggregate?> FindCorrelatedPaymentAsync(
        PaymentVerificationResult result,
        CancellationToken cancellationToken)
    {
        IQueryable<PaymentAttempt> query = _db.PaymentAttempts.Where(item => item.ProviderKey == result.ProviderKey);
        if (result.TransactionReference is { } transaction)
        {
            query = query.Where(item => item.ProviderTransactionReference == transaction);
        }
        else if (result.RequestReference is { } request)
        {
            query = query.Where(item => item.ProviderRequestReference == request);
        }
        else
        {
            return null;
        }

        var attempt = await query.SingleOrDefaultAsync(cancellationToken);
        if (attempt is null)
        {
            return null;
        }

        var paymentId = _db.Entry(attempt).Property<PaymentId>("PaymentId").CurrentValue;
        return await _db.Payments
            .Include(item => item.Attempts)
            .Include(item => item.ExecutionSnapshot)
            .SingleAsync(item => item.Id == paymentId, cancellationToken);
    }

    private static PaymentAttempt? FindCorrelatedAttempt(
        PaymentAggregate payment,
        PaymentVerificationResult result)
    {
        return payment.Attempts.SingleOrDefault(item =>
            item.ProviderKey.Equals(result.ProviderKey)
            && ((result.TransactionReference is { } transaction
                    && item.ProviderTransactionReference.Equals(transaction))
                || (result.TransactionReference is null
                    && result.RequestReference is { } request
                    && item.ProviderRequestReference.Equals(request))));
    }

    private async Task<Refund?> FindCorrelatedRefundAsync(
        PaymentVerificationResult result,
        CancellationToken cancellationToken)
    {
        IQueryable<RefundAttempt> query = _db.RefundAttempts.Where(item => item.ProviderKey == result.ProviderKey);
        if (result.TransactionReference is { } transaction)
        {
            query = query.Where(item => item.ProviderTransactionReference == transaction);
        }
        else if (result.RequestReference is { } request)
        {
            query = query.Where(item => item.ProviderRequestReference == request);
        }
        else
        {
            return null;
        }

        var attempt = await query.SingleOrDefaultAsync(cancellationToken);
        if (attempt is null)
        {
            return null;
        }

        var refundId = _db.Entry(attempt).Property<RefundId>("RefundId").CurrentValue;
        return await _db.Refunds
            .Include(item => item.Attempts)
            .Include(item => item.Amount)
            .SingleAsync(item => item.Id == refundId, cancellationToken);
    }

    private static RefundAttempt? FindCorrelatedRefundAttempt(
        Refund refund,
        PaymentVerificationResult result)
    {
        return refund.Attempts.SingleOrDefault(item =>
            item.ProviderKey.Equals(result.ProviderKey)
            && ((result.TransactionReference is { } transaction
                    && item.ProviderTransactionReference.Equals(transaction))
                || (result.TransactionReference is null
                    && result.RequestReference is { } request
                    && item.ProviderRequestReference.Equals(request))));
    }
}
