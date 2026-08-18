using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Database-backed GetOrCreate: one Succeeded Payment -> one logical full Refund (P20-R6).
/// Amount/currency come only from PaymentExecutionSnapshot.
/// </summary>
internal sealed class RefundGetOrCreateService
{
    private readonly PaymentDbContext _db;
    private readonly IClock _clock;

    public RefundGetOrCreateService(PaymentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Refund> GetOrCreateAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindAsync(paymentId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var payment = await LoadPaymentAsync(paymentId, cancellationToken);
        var created = Refund.CreateForSucceededPayment(payment, _clock.GetCurrentInstant());
        _db.Refunds.Add(created);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            return await FindAsync(paymentId, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent Refund create did not converge.");
        }
    }

    private Task<Refund?> FindAsync(PaymentId paymentId, CancellationToken cancellationToken) =>
        _db.Refunds
            .Include(item => item.Attempts)
            .Include(item => item.Amount)
            .SingleOrDefaultAsync(item => item.PaymentId == paymentId, cancellationToken);

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
