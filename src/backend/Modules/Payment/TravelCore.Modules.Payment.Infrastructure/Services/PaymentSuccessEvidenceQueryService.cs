using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

internal sealed class PaymentSuccessEvidenceQueryService : IPaymentSuccessEvidenceQuery
{
    private readonly PaymentDbContext _db;

    public PaymentSuccessEvidenceQueryService(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.Booking.BookingId == bookingId, cancellationToken);
        if (payment?.ExecutionSnapshot is null)
        {
            return null;
        }

        return new PaymentSuccessEvidenceRead(
            payment.Id.Value,
            payment.Booking.BookingId,
            payment.Status.ToString(),
            payment.ExecutionSnapshot.Amount.Amount,
            payment.ExecutionSnapshot.Amount.Currency.Value,
            payment.Status == PaymentStatus.Succeeded);
    }
}
