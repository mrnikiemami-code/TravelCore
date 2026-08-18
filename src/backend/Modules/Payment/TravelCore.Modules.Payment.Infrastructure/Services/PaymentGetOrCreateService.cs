using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Database-backed GetOrCreate: one Booking -> one logical Payment (P20-R4).
/// </summary>
internal sealed class PaymentGetOrCreateService
{
    private readonly PaymentDbContext _db;
    private readonly IClock _clock;

    public PaymentGetOrCreateService(PaymentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PaymentAggregate> GetOrCreateAsync(
        BookingReference booking,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindAsync(booking, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = PaymentAggregate.Create(booking, _clock.GetCurrentInstant());
        _db.Payments.Add(created);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            _db.Entry(created).State = EntityState.Detached;
            foreach (var attempt in created.Attempts)
            {
                _db.Entry(attempt).State = EntityState.Detached;
            }

            return await FindAsync(booking, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent Payment create did not converge.");
        }
    }

    private Task<PaymentAggregate?> FindAsync(BookingReference booking, CancellationToken cancellationToken) =>
        _db.Payments
            .Include(item => item.Attempts)
            .SingleOrDefaultAsync(item => item.Booking == booking, cancellationToken);
}
