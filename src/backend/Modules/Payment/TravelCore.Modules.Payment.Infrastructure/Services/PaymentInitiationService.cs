using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure.Options;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Trusted internal initiation with GetOrCreate, scoped idempotency, and no retry on ambiguity (P20-R4).
/// </summary>
internal sealed class PaymentInitiationService : IHotelBookingPaymentInitiationService
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IOptions<PaymentProviderOptions> _options;
    private readonly IClock _clock;
    private readonly PaymentGetOrCreateService _getOrCreate;
    private readonly PaymentPreparationService? _preparation;

    public PaymentInitiationService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IOptions<PaymentProviderOptions> options,
        IClock clock,
        PaymentGetOrCreateService getOrCreate,
        PaymentPreparationService? preparation = null)
    {
        _db = db;
        _resolver = resolver;
        _options = options;
        _clock = clock;
        _getOrCreate = getOrCreate;
        _preparation = preparation;
    }

    public Task<PaymentInitiationResult> InitiateAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken = default) =>
        InitiateAsync(paymentId, idempotencyKey: null, cancellationToken);

    public async Task<PaymentInitiationResult> InitiateForBookingAsync(
        BookingReference booking,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var payment = await _getOrCreate.GetOrCreateAsync(booking, cancellationToken);
        return await InitiateAsync(payment.Id, idempotencyKey, cancellationToken);
    }

    public async Task<PaymentInitiationResult> InitiateForHotelBookingAsync(
        HotelBookingPaymentReference hotelBooking,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var payment = await _getOrCreate.GetOrCreateAsync(hotelBooking, cancellationToken);
        return await InitiateAsync(payment.Id, idempotencyKey, cancellationToken);
    }

    public async Task<PaymentInitiationResult> InitiateAsync(
        PaymentId paymentId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (_preparation is not null)
        {
            await _preparation.PrepareAsync(paymentId, cancellationToken);
        }
        var payment = await LoadAsync(paymentId, cancellationToken);
        if (payment.Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Payment already succeeded.");
        }

        if (payment.ExecutionSnapshot is null)
        {
            throw new InvalidOperationException("PaymentExecutionSnapshot must be prepared before initiation.");
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var key = PaymentInitiationIdempotencyRecord.Normalize(idempotencyKey);
            var existing = await _db.InitiationIdempotency.SingleOrDefaultAsync(
                item => item.PaymentId == paymentId && item.IdempotencyKey == key,
                cancellationToken);
            if (existing is not null)
            {
                return RecoverAttempt(payment, existing.AttemptId, providerKey: null);
            }
        }

        var active = payment.Attempts.SingleOrDefault(item => item.IsActive);
        if (active is not null)
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new InvalidOperationException("Unresolved PaymentAttempt blocks retry.");
            }

            return await ExecuteProviderAsync(payment, active, cancellationToken);
        }

        if (!ProviderKey.TryParse(_options.Value.DefaultProviderKey, out _))
        {
            throw new InvalidOperationException("A server-configured ProviderKey is required for initiation.");
        }

        var now = _clock.GetCurrentInstant();
        PaymentAttempt attempt;
        try
        {
            attempt = payment.CreateAttempt(now);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                _db.InitiationIdempotency.Add(
                    PaymentInitiationIdempotencyRecord.Create(
                        payment.Id,
                        idempotencyKey,
                        attempt.Id,
                        now));
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            payment = await LoadAsync(paymentId, cancellationToken);
            if (payment.Status == PaymentStatus.Succeeded)
            {
                throw new InvalidOperationException("Payment already succeeded.");
            }

            attempt = payment.Attempts.SingleOrDefault(item => item.IsActive)
                ?? throw new InvalidOperationException("Concurrent PaymentAttempt create did not converge.");
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new InvalidOperationException("Unresolved PaymentAttempt blocks retry.");
            }
        }

        payment = await LoadAsync(paymentId, cancellationToken);
        attempt = payment.Attempts.Single(item => item.Id.Equals(attempt.Id));
        return await ExecuteProviderAsync(payment, attempt, cancellationToken);
    }

    private async Task<PaymentInitiationResult> ExecuteProviderAsync(
        PaymentAggregate payment,
        PaymentAttempt attempt,
        CancellationToken cancellationToken)
    {
        if (!ProviderKey.TryParse(_options.Value.DefaultProviderKey, out var providerKey))
        {
            throw new InvalidOperationException("A server-configured ProviderKey is required for initiation.");
        }
        var execution = payment.ExecutionSnapshot
            ?? throw new InvalidOperationException("PaymentExecutionSnapshot must be prepared before initiation.");

        var gateway = _resolver.Resolve(providerKey)
            ?? throw new InvalidOperationException("Configured Payment provider is not registered.");
        if (_resolver.Check(providerKey, PaymentProviderCapability.RedirectInitiation)
            == ProviderCapabilityStatus.UnsupportedCapability)
        {
            throw new InvalidOperationException("Provider does not support RedirectInitiation.");
        }

        PaymentInitiationResult result;
        try
        {
            result = await gateway.InitiatePaymentAsync(
                new PaymentInitiationRequest(
                    payment.Id.Value,
                    attempt.Id.Value,
                    payment.TargetReferenceId,
                    providerKey,
                    execution.Amount.Amount,
                    execution.Amount.Currency.Value),
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

        payment = await LoadAsync(payment.Id, cancellationToken);
        if (payment.Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Payment already succeeded.");
        }

        attempt = payment.Attempts.Single(item => item.Id.Equals(attempt.Id));
        VerifiedProviderOutcomeApplier.ApplyInitiation(payment, attempt, result, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private PaymentInitiationResult RecoverAttempt(
        PaymentAggregate payment,
        PaymentAttemptId attemptId,
        ProviderKey? providerKey)
    {
        var attempt = payment.Attempts.Single(item => item.Id.Equals(attemptId));
        var key = attempt.ProviderKey
            ?? providerKey
            ?? (ProviderKey.TryParse(_options.Value.DefaultProviderKey, out var configured)
                ? configured
                : throw new InvalidOperationException("A server-configured ProviderKey is required for initiation."));
        return new PaymentInitiationResult
        {
            Outcome = attempt.Status == PaymentAttemptStatus.Failed
                ? PaymentInitiationOutcome.DefinitiveFailure
                : attempt.Status == PaymentAttemptStatus.Initiated
                    ? PaymentInitiationOutcome.Initiated
                    : PaymentInitiationOutcome.Unknown,
            ProviderKey = key,
            RequestReference = attempt.ProviderRequestReference,
            TransactionReference = attempt.ProviderTransactionReference,
        };
    }

    private async Task<PaymentAggregate> LoadAsync(PaymentId paymentId, CancellationToken cancellationToken)
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
