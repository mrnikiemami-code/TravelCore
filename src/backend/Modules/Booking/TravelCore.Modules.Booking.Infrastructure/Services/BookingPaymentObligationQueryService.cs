using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned authoritative payable obligation query for Payment preparation (P20-R5).
/// </summary>
internal sealed class BookingPaymentObligationQueryService : IBookingPaymentObligationQuery
{
    private readonly BookingDbContext _db;

    public BookingPaymentObligationQueryService(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<BookingPaymentObligationRead?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.MonetarySnapshot)
            .SingleOrDefaultAsync(x => x.Id == BookingId.From(bookingId), cancellationToken);
        if (booking is null || booking.MonetarySnapshot is null)
        {
            return null;
        }

        var hasActiveHold = await _db.CapacityHolds.AnyAsync(
            x => x.BookingId == booking.Id
                && x.Status == CapacityHoldStatus.Active,
            cancellationToken);
        var eligible = booking.Status == BookingStatus.Pending && hasActiveHold;

        return new BookingPaymentObligationRead(
            booking.Id.Value,
            booking.Status.ToString(),
            booking.MonetarySnapshot.Total.Amount,
            booking.MonetarySnapshot.Total.Currency.Value,
            booking.MonetarySnapshot.Id.Value,
            eligible);
    }
}
