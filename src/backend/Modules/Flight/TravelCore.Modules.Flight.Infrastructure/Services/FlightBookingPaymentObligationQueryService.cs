using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// Flight-owned payable obligation. Amount/currency come only from FlightBookingMonetarySnapshot.
/// Payment is eligible only after Confirmed supplier reservation and before ticketing/reservation expiry.
/// </summary>
internal sealed class FlightBookingPaymentObligationQueryService : IFlightBookingPaymentObligationQuery
{
    private readonly FlightDbContext _db;
    private readonly IClock _clock;

    public FlightBookingPaymentObligationQueryService(FlightDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<FlightBookingPaymentObligationRead?> GetByFlightBookingIdAsync(
        Guid flightBookingId,
        CancellationToken cancellationToken = default)
    {
        var id = FlightBookingId.From(flightBookingId);
        var booking = await _db.FlightBookings
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var snapshot = await _db.FlightOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .Include(x => x.FareRules)
            .SingleOrDefaultAsync(x => x.FlightBookingId == id, cancellationToken);
        if (snapshot?.Monetary is null)
        {
            return null;
        }

        var reservation = await _db.FlightSupplierReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FlightBookingId == id, cancellationToken);

        var now = _clock.GetCurrentInstant();
        var reservationOk = reservation is { Status: FlightSupplierReservationStatus.Confirmed }
            && (reservation.ReservationExpiresAt is null || reservation.ReservationExpiresAt > now);
        var deadlineOk = snapshot.FareRules.TicketingDeadline is null
            || snapshot.FareRules.TicketingDeadline > now;
        var eligible = booking.Status == FlightBookingStatus.Pending
            && reservationOk
            && deadlineOk;

        return new FlightBookingPaymentObligationRead(
            booking.Id.Value,
            booking.Status.ToString(),
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.Total.Currency.Value,
            snapshot.Id.Value,
            eligible);
    }
}
