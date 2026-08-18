using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Internal read-only operational Payment/Refund query (P20-R8).
/// Does not mutate financial truth. Does not expose passenger, contact, or access tokens.
/// </summary>
internal sealed class PaymentOperationalQueryService : IPaymentOperationalQuery
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentProviderResolver _resolver;
    private readonly PaymentAttemptRecheckService _paymentRecheck;
    private readonly RefundAttemptRecheckService _refundRecheck;

    public PaymentOperationalQueryService(
        PaymentDbContext db,
        IPaymentProviderResolver resolver,
        PaymentAttemptRecheckService paymentRecheck,
        RefundAttemptRecheckService refundRecheck)
    {
        _db = db;
        _resolver = resolver;
        _paymentRecheck = paymentRecheck;
        _refundRecheck = refundRecheck;
    }

    public async Task<PaymentOperationalRead?> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var id = PaymentId.From(paymentId);
        var payment = await _db.Payments
            .AsNoTracking()
            .Include(x => x.Attempts)
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        var refund = await _db.Refunds
            .AsNoTracking()
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.PaymentId == id, cancellationToken);
        var paymentIssues = await _db.ReconciliationIssues
            .AsNoTracking()
            .Where(x => x.PaymentId == id)
            .Select(x => x.Kind.ToString())
            .ToListAsync(cancellationToken);
        var refundIssues = refund is null
            ? []
            : await _db.RefundReconciliationIssues
                .AsNoTracking()
                .Where(x => x.RefundId == refund.Id)
                .Select(x => x.Kind.ToString())
                .ToListAsync(cancellationToken);

        var collectionKey = payment.Attempts
            .FirstOrDefault(x => x.Status == PaymentAttemptStatus.Succeeded)?.ProviderKey
            ?? payment.Attempts.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.ProviderKey;
        var descriptor = collectionKey is { } key ? _resolver.Describe(key) : null;
        var compensation = refund is { Status: RefundStatus.Succeeded }
            ? "RefundSucceeded"
            : refund is { Status: RefundStatus.Pending }
                ? descriptor is { } d && !d.Capabilities.HasFlag(PaymentProviderCapability.RefundInitiation)
                    ? "RefundCapabilityUnavailable"
                    : "CompensationPending"
                : payment.Status == PaymentStatus.Succeeded
                    ? "Succeeded"
                    : null;

        return new PaymentOperationalRead(
            payment.Id.Value,
            payment.Booking?.BookingId ?? Guid.Empty,
            payment.Status.ToString(),
            payment.ExecutionSnapshot?.Amount.Amount,
            payment.ExecutionSnapshot?.Amount.Currency.Value,
            payment.CreatedAt.ToDateTimeOffset(),
            payment.SucceededAt?.ToDateTimeOffset(),
            payment.Attempts
                .OrderBy(x => x.CreatedAt)
                .Select(x => new PaymentAttemptOperationalRead(
                    x.Id.Value,
                    x.Status.ToString(),
                    x.ProviderKey?.Value,
                    x.ProviderRequestReference?.Value,
                    x.ProviderTransactionReference?.Value,
                    x.CreatedAt.ToDateTimeOffset(),
                    x.InitiatedAt?.ToDateTimeOffset()))
                .ToArray(),
            refund is null
                ? null
                : new RefundOperationalRead(
                    refund.Id.Value,
                    refund.Status.ToString(),
                    refund.Amount.Amount,
                    refund.Amount.Currency.Value,
                    refund.Attempts
                        .OrderBy(x => x.CreatedAt)
                        .Select(x => new RefundAttemptOperationalRead(
                            x.Id.Value,
                            x.Status.ToString(),
                            x.ProviderKey?.Value,
                            x.ProviderRequestReference?.Value,
                            x.ProviderTransactionReference?.Value,
                            x.CreatedAt.ToDateTimeOffset(),
                            x.InitiatedAt?.ToDateTimeOffset()))
                        .ToArray(),
                    refundIssues),
            paymentIssues,
            descriptor,
            compensation,
            payment.TargetKind.ToString(),
            payment.TargetReferenceId);
    }

    public async Task<ProviderCapabilityStatus> RecheckPaymentAttemptAsync(
        Guid attemptId,
        CancellationToken cancellationToken = default)
    {
        var id = PaymentAttemptId.From(attemptId);
        var payment = await _db.Payments
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.Attempts.Any(a => a.Id == id), cancellationToken);
        if (payment is null)
        {
            return ProviderCapabilityStatus.UnknownProvider;
        }

        var attempt = payment.Attempts.Single(x => x.Id.Equals(id));
        if (attempt.ProviderKey is not { } key)
        {
            return ProviderCapabilityStatus.UnknownProvider;
        }

        var status = _resolver.Check(key, PaymentProviderCapability.PaymentStatusQuery);
        if (status != ProviderCapabilityStatus.Available)
        {
            return status;
        }

        await _paymentRecheck.RecheckAsync(id, cancellationToken);
        return ProviderCapabilityStatus.Available;
    }

    public async Task<ProviderCapabilityStatus> RecheckRefundAttemptAsync(
        Guid attemptId,
        CancellationToken cancellationToken = default)
    {
        var id = RefundAttemptId.From(attemptId);
        var refund = await _db.Refunds
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.Attempts.Any(a => a.Id == id), cancellationToken);
        if (refund is null)
        {
            return ProviderCapabilityStatus.UnknownProvider;
        }

        var attempt = refund.Attempts.Single(x => x.Id.Equals(id));
        if (attempt.ProviderKey is not { } key)
        {
            return ProviderCapabilityStatus.UnknownProvider;
        }

        var status = _resolver.Check(key, PaymentProviderCapability.RefundStatusQuery);
        if (status != ProviderCapabilityStatus.Available)
        {
            return status;
        }

        await _refundRecheck.RecheckAsync(id, cancellationToken);
        return ProviderCapabilityStatus.Available;
    }
}
