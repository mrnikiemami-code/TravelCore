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
        var reference = new BookingReference(bookingId);
        var payment = await _db.Payments
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.Booking == reference, cancellationToken);
        if (payment?.ExecutionSnapshot is null || payment.Booking is null)
        {
            return null;
        }

        return new PaymentSuccessEvidenceRead(
            payment.Id.Value,
            payment.Booking.Value.BookingId,
            payment.Status.ToString(),
            payment.ExecutionSnapshot.Amount.Amount,
            payment.ExecutionSnapshot.Amount.Currency.Value,
            payment.Status == PaymentStatus.Succeeded);
    }

    public async Task<HotelBookingPaymentSuccessEvidenceRead?> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default)
    {
        var reference = new HotelBookingPaymentReference(hotelBookingId);
        var payment = await _db.Payments
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.HotelBooking == reference, cancellationToken);
        if (payment?.ExecutionSnapshot is null || payment.HotelBooking is null)
        {
            return null;
        }

        return new HotelBookingPaymentSuccessEvidenceRead(
            payment.Id.Value,
            payment.HotelBooking.Value.HotelBookingId,
            payment.Status.ToString(),
            payment.ExecutionSnapshot.Amount.Amount,
            payment.ExecutionSnapshot.Amount.Currency.Value,
            payment.Status == PaymentStatus.Succeeded);
    }
}
