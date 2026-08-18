using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Provider-neutral refund initiation. Does not hold a DB transaction over the network (P20-R6).
/// Unresolved attempts block another initiate. Uses the collection provider, not a failover.
/// </summary>
internal sealed class RefundInitiationService
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IClock _clock;

    public RefundInitiationService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<PaymentInitiationResult?> InitiateAsync(
        RefundId refundId,
        CancellationToken cancellationToken = default)
    {
        var refund = await LoadRefundAsync(refundId, cancellationToken);
        if (refund.Status == RefundStatus.Succeeded)
        {
            return null;
        }

        var payment = await LoadPaymentAsync(refund.PaymentId, cancellationToken);
        var collection = payment.Attempts.SingleOrDefault(item => item.Status == PaymentAttemptStatus.Succeeded)
            ?? throw new InvalidOperationException("Refund requires a successful PaymentAttempt provider context.");
        var providerKey = collection.ProviderKey
            ?? throw new InvalidOperationException("Refund requires the original collection ProviderKey.");
        var capability = _resolver.Check(providerKey, PaymentProviderCapability.RefundInitiation);
        if (capability is ProviderCapabilityStatus.UnknownProvider or ProviderCapabilityStatus.DisabledProvider)
        {
            throw new InvalidOperationException("Configured Payment provider is not registered.");
        }

        if (capability == ProviderCapabilityStatus.UnsupportedCapability)
        {
            throw new InvalidOperationException("Provider does not support RefundInitiation.");
        }

        var active = refund.Attempts.SingleOrDefault(item => item.IsActive);
        if (active is not null)
        {
            if (active.Status == RefundAttemptStatus.Initiated || active.ProviderKey is not null)
            {
                return RecoverAttempt(active, providerKey);
            }

            return await ExecuteProviderAsync(payment, refund, active, providerKey, collection, cancellationToken);
        }

        RefundAttempt attempt;
        try
        {
            attempt = refund.CreateAttempt(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            refund = await LoadRefundAsync(refundId, cancellationToken);
            if (refund.Status == RefundStatus.Succeeded)
            {
                return null;
            }

            attempt = refund.Attempts.SingleOrDefault(item => item.IsActive)
                ?? throw new InvalidOperationException("Concurrent RefundAttempt create did not converge.");
            if (attempt.Status == RefundAttemptStatus.Initiated || attempt.ProviderKey is not null)
            {
                return RecoverAttempt(attempt, providerKey);
            }
        }

        return await ExecuteProviderAsync(payment, refund, attempt, providerKey, collection, cancellationToken);
    }

    private async Task<PaymentInitiationResult> ExecuteProviderAsync(
        PaymentAggregate payment,
        Refund refund,
        RefundAttempt attempt,
        ProviderKey providerKey,
        PaymentAttempt collection,
        CancellationToken cancellationToken)
    {
        var gateway = _resolver.Resolve(providerKey)
            ?? throw new InvalidOperationException("Configured Payment provider is not registered.");
        if (_resolver.Check(providerKey, PaymentProviderCapability.RefundInitiation)
            == ProviderCapabilityStatus.UnsupportedCapability)
        {
            throw new InvalidOperationException("Provider does not support RefundInitiation.");
        }

        PaymentInitiationResult result;
        try
        {
            result = await gateway.InitiateRefundAsync(
                new RefundInitiationRequest(
                    refund.Id.Value,
                    attempt.Id.Value,
                    payment.Id.Value,
                    payment.Booking.BookingId,
                    providerKey,
                    collection.ProviderTransactionReference,
                    refund.Amount.Amount,
                    refund.Amount.Currency.Value),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            result = new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.Unknown,
                ProviderKey = providerKey,
            };
        }

        refund = await LoadRefundAsync(refund.Id, cancellationToken);
        if (refund.Status == RefundStatus.Succeeded)
        {
            return result;
        }

        attempt = refund.Attempts.Single(item => item.Id.Equals(attempt.Id));
        VerifiedRefundOutcomeApplier.ApplyInitiation(refund, attempt, result, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static PaymentInitiationResult RecoverAttempt(RefundAttempt attempt, ProviderKey providerKey)
    {
        var key = attempt.ProviderKey ?? providerKey;
        return new PaymentInitiationResult
        {
            Outcome = attempt.Status == RefundAttemptStatus.Failed
                ? PaymentInitiationOutcome.DefinitiveFailure
                : attempt.Status == RefundAttemptStatus.Initiated
                    ? PaymentInitiationOutcome.Initiated
                    : PaymentInitiationOutcome.Unknown,
            ProviderKey = key,
            RequestReference = attempt.ProviderRequestReference,
            TransactionReference = attempt.ProviderTransactionReference,
        };
    }

    private async Task<Refund> LoadRefundAsync(RefundId refundId, CancellationToken cancellationToken)
    {
        var refund = await _db.Refunds
            .Include(item => item.Attempts)
            .Include(item => item.Amount)
            .SingleOrDefaultAsync(item => item.Id == refundId, cancellationToken);
        if (refund is null)
        {
            throw new InvalidOperationException("Refund was not found.");
        }

        return refund;
    }

    private async Task<PaymentAggregate> LoadPaymentAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await _db.Payments
            .Include(item => item.Attempts)
            .Include(item => item.ExecutionSnapshot)
            .SingleOrDefaultAsync(item => item.Id == paymentId, cancellationToken);
        if (payment is null)
        {
            throw new InvalidOperationException("Payment was not found.");
        }

        return payment;
    }
}
