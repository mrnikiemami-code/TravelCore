using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure.Options;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Trusted internal initiation: persist Created, call provider outside a DB transaction, then persist outcome (P20-R3).
/// Not a public API.
/// </summary>
internal sealed class PaymentInitiationService
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly IOptions<PaymentProviderOptions> _options;
    private readonly IClock _clock;

    public PaymentInitiationService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        IOptions<PaymentProviderOptions> options,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _options = options;
        _clock = clock;
    }

    public async Task<PaymentInitiationResult> InitiateAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken = default)
    {
        if (!ProviderKey.TryParse(_options.Value.DefaultProviderKey, out var providerKey))
        {
            throw new InvalidOperationException("A server-configured ProviderKey is required for initiation.");
        }

        var gateway = _resolver.Resolve(providerKey)
            ?? throw new InvalidOperationException("Configured Payment provider is not registered.");

        var payment = await LoadAsync(paymentId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var attempt = payment.Attempts.SingleOrDefault(item => item.IsActive)
            ?? payment.CreateAttempt(now);
        await _db.SaveChangesAsync(cancellationToken);

        PaymentInitiationResult result;
        try
        {
            result = await gateway.InitiatePaymentAsync(
                new PaymentInitiationRequest(
                    payment.Id.Value,
                    attempt.Id.Value,
                    payment.Booking.BookingId,
                    providerKey),
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

        payment = await LoadAsync(paymentId, cancellationToken);
        attempt = payment.Attempts.Single(item => item.Id.Equals(attempt.Id));
        VerifiedProviderOutcomeApplier.ApplyInitiation(payment, attempt, result, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<PaymentAggregate> LoadAsync(PaymentId paymentId, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments
            .Include(item => item.Attempts)
            .SingleOrDefaultAsync(item => item.Id == paymentId, cancellationToken);
        if (payment is null)
        {
            throw new InvalidOperationException("Payment was not found.");
        }

        return payment;
    }
}
