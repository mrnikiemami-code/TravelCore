using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// HotelBooking-owned payable obligation. Amount/currency come only from HotelBookingMonetarySnapshot.
/// </summary>
internal sealed class HotelBookingPaymentObligationQueryService : IHotelBookingPaymentObligationQuery
{
    private readonly HotelBookingDbContext _db;
    private readonly IClock _clock;

    public HotelBookingPaymentObligationQueryService(HotelBookingDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<HotelBookingPaymentObligationRead?> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default)
    {
        var id = HotelBookingId.From(hotelBookingId);
        var booking = await _db.HotelBookings
            .AsNoTracking()
            .Include(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var snapshot = await _db.HotelRateOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .SingleOrDefaultAsync(x => x.HotelBookingId == id, cancellationToken);
        if (snapshot?.Monetary is null)
        {
            return null;
        }

        var now = _clock.GetCurrentInstant();
        var holds = await _db.HotelAvailabilityHolds
            .AsNoTracking()
            .Include(x => x.Rooms)
            .Where(x => x.HotelBookingId == id)
            .ToListAsync(cancellationToken);
        var expectedRooms = booking.Rooms.Select(r => r.Id).ToHashSet();
        var eligible = booking.Status == HotelBookingStatus.Pending
            && holds.Any(hold =>
                hold.IsActiveAndUnexpired(now)
                && hold.Rooms.Select(r => r.RoomReservationId).ToHashSet().SetEquals(expectedRooms));

        return new HotelBookingPaymentObligationRead(
            booking.Id.Value,
            booking.Status.ToString(),
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.Total.Currency.Value,
            snapshot.Id.Value,
            eligible);
    }
}
