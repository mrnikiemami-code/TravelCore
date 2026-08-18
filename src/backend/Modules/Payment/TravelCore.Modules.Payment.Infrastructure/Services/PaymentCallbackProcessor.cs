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

        var payment = await FindCorrelatedPaymentAsync(verification.Result, cancellationToken);
        if (payment is null)
        {
            return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.UnknownAttempt);
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
            _clock.GetCurrentInstant());
        if (status is VerificationApplyStatus.AmountMismatch or VerificationApplyStatus.CurrencyMismatch)
        {
            _db.ReconciliationIssues.Add(
                PaymentReconciliationIssue.Create(
                    payment.Id,
                    attempt.Id,
                    status == VerificationApplyStatus.AmountMismatch
                        ? PaymentReconciliationIssueKind.AmountMismatch
                        : PaymentReconciliationIssueKind.CurrencyMismatch,
                    _clock.GetCurrentInstant()));
        }
        await _db.SaveChangesAsync(cancellationToken);
        return new PaymentCallbackProcessResult(PaymentCallbackProcessStatus.Applied);
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
}
