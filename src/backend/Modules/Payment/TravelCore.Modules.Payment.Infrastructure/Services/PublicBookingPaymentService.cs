using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure.Options;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Booking-scoped public Payment status and initiation (TC-P20-T007 / P20-R7).
/// Does not accept client amount/currency/success. Does not use test/fake as production.
/// </summary>
internal sealed class PublicBookingPaymentService : IPublicBookingPaymentService
{
    private readonly PaymentDbContext _db;
    private readonly PaymentGetOrCreateService _getOrCreate;
    private readonly PaymentPreparationService _preparation;
    private readonly PaymentInitiationService _initiation;
    private readonly IOptions<PaymentProviderOptions> _options;

    public PublicBookingPaymentService(
        PaymentDbContext db,
        PaymentGetOrCreateService getOrCreate,
        PaymentPreparationService preparation,
        PaymentInitiationService initiation,
        IOptions<PaymentProviderOptions> options)
    {
        _db = db;
        _getOrCreate = getOrCreate;
        _preparation = preparation;
        _initiation = initiation;
        _options = options;
    }

    public async Task<PublicPaymentRead> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var payment = await LoadOrCreateAsync(bookingId, cancellationToken);
        await TryPrepareAsync(payment.Id, cancellationToken);
        return await MapAsync(payment.Id, redirectUri: null, cancellationToken);
    }

    public async Task<PublicPaymentCommandResult> InitiateForBookingAsync(
        Guid bookingId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var payment = await LoadOrCreateAsync(bookingId, cancellationToken);
        if (payment.Status == PaymentStatus.Succeeded)
        {
            return new PublicPaymentCommandResult(
                await MapAsync(payment.Id, redirectUri: null, cancellationToken),
                PublicPaymentCommandStatus.Completed);
        }

        try
        {
            await _preparation.PrepareAsync(payment.Id, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return new PublicPaymentCommandResult(
                await MapAsync(payment.Id, redirectUri: null, cancellationToken),
                PublicPaymentCommandStatus.BookingIneligible);
        }

        payment = await LoadAsync(payment.Id, cancellationToken);
        var active = payment.Attempts.SingleOrDefault(x => x.IsActive);
        if (active is not null && active.Status != PaymentAttemptStatus.Failed)
        {
            return new PublicPaymentCommandResult(
                await MapAsync(payment.Id, redirectUri: null, cancellationToken),
                PublicPaymentCommandStatus.Completed);
        }

        if (!IsProductionProviderConfigured())
        {
            return new PublicPaymentCommandResult(
                await MapAsync(payment.Id, redirectUri: null, cancellationToken),
                PublicPaymentCommandStatus.ProviderUnavailable);
        }

        var result = await _initiation.InitiateForBookingAsync(
            new BookingReference(bookingId),
            idempotencyKey,
            cancellationToken);
        return new PublicPaymentCommandResult(
            await MapAsync(payment.Id, result.RedirectUri?.ToString(), cancellationToken),
            PublicPaymentCommandStatus.Completed);
    }

    private async Task<PaymentAggregate> LoadOrCreateAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var created = await _getOrCreate.GetOrCreateAsync(new BookingReference(bookingId), cancellationToken);
        return await LoadAsync(created.Id, cancellationToken);
    }

    private async Task TryPrepareAsync(PaymentId paymentId, CancellationToken cancellationToken)
    {
        var payment = await LoadAsync(paymentId, cancellationToken);
        if (payment.Status == PaymentStatus.Succeeded || payment.ExecutionSnapshot is not null)
        {
            return;
        }

        try
        {
            await _preparation.PrepareAsync(paymentId, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Public GET remains readable; ineligible Booking cannot bind a snapshot.
        }
    }

    private async Task<PublicPaymentRead> MapAsync(
        PaymentId paymentId,
        string? redirectUri,
        CancellationToken cancellationToken)
    {
        var payment = await LoadAsync(paymentId, cancellationToken);
        var refund = await _db.Refunds
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PaymentId == payment.Id, cancellationToken);
        var latest = payment.Attempts
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        var production = IsProductionProviderConfigured();
        return new PublicPaymentRead(
            payment.Id.Value,
            payment.Status.ToString(),
            payment.ExecutionSnapshot?.Amount.Amount,
            payment.ExecutionSnapshot?.Amount.Currency.Value,
            production && payment.Status == PaymentStatus.Pending,
            latest?.Status.ToString(),
            refund?.Status.ToString(),
            ResolveSafeAction(payment, latest, refund, production),
            production ? redirectUri : null);
    }

    private async Task<PaymentAggregate> LoadAsync(PaymentId paymentId, CancellationToken cancellationToken)
    {
        return await _db.Payments
            .Include(x => x.Attempts)
            .Include(x => x.ExecutionSnapshot)
            .SingleAsync(x => x.Id == paymentId, cancellationToken);
    }

    private bool IsProductionProviderConfigured()
    {
        if (!PaymentProviderTrustBoundary.NamedProductionAdapterImplemented)
        {
            return false;
        }

        if (!ProviderKey.TryParse(_options.Value.DefaultProviderKey, out var key))
        {
            return false;
        }

        return !string.Equals(key.Value, "test", StringComparison.Ordinal);
    }

    private static string ResolveSafeAction(
        PaymentAggregate payment,
        PaymentAttempt? latest,
        Refund? refund,
        bool production)
    {
        if (refund is { Status: RefundStatus.Succeeded })
        {
            return "RefundSucceeded";
        }

        if (refund is { Status: RefundStatus.Pending })
        {
            return "CompensationPending";
        }

        if (payment.Status == PaymentStatus.Succeeded)
        {
            return "Succeeded";
        }

        if (latest is { Status: PaymentAttemptStatus.Initiated or PaymentAttemptStatus.Created })
        {
            return "Wait";
        }

        if (latest is { Status: PaymentAttemptStatus.Failed } && production)
        {
            return "Retry";
        }

        if (production)
        {
            return "Initiate";
        }

        return "Unavailable";
    }
}
