using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Internal read-only operational HotelBooking query (P21-R8).
/// Does not mutate booking/financial truth. Redacts guest/contact PII and tokens.
/// </summary>
internal sealed class HotelBookingOperationalQueryService : IHotelBookingOperationalQuery
{
    private readonly HotelBookingDbContext _db;
    private readonly IHotelAvailabilitySourceResolver _availability;
    private readonly IHotelReservationSourceResolver _reservations;
    private readonly HotelAvailabilityHoldService _holds;
    private readonly HotelSupplierReservationService _reservationService;
    private readonly HotelBookingCancellationService _cancellations;
    private readonly IPaymentSuccessEvidenceQuery _paymentEvidence;

    public HotelBookingOperationalQueryService(
        HotelBookingDbContext db,
        IHotelAvailabilitySourceResolver availability,
        IHotelReservationSourceResolver reservations,
        HotelAvailabilityHoldService holds,
        HotelSupplierReservationService reservationService,
        HotelBookingCancellationService cancellations,
        IPaymentSuccessEvidenceQuery paymentEvidence)
    {
        _db = db;
        _availability = availability;
        _reservations = reservations;
        _holds = holds;
        _reservationService = reservationService;
        _cancellations = cancellations;
        _paymentEvidence = paymentEvidence;
    }

    public async Task<HotelBookingOperationalRead?> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default)
    {
        if (hotelBookingId == Guid.Empty)
        {
            return null;
        }

        var id = HotelBookingId.From(hotelBookingId);
        var booking = await _db.HotelBookings
            .AsNoTracking()
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Guests)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var hold = await _db.HotelAvailabilityHolds
            .AsNoTracking()
            .Where(x => x.HotelBookingId == id)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var offer = await _db.HotelRateOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .SingleOrDefaultAsync(x => x.HotelBookingId == id, cancellationToken);
        var reservation = await _db.HotelSupplierReservations
            .AsNoTracking()
            .Include(x => x.Attempts)
            .Where(x => x.HotelBookingId == id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var cancellation = await _db.HotelBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.HotelBookingId == id, cancellationToken);
        var issues = await _db.HotelBookingReconciliationIssues
            .AsNoTracking()
            .Where(x => x.HotelBookingId == id)
            .Select(x => x.Kind.ToString())
            .ToListAsync(cancellationToken);
        var payment = await _paymentEvidence.GetByHotelBookingIdAsync(hotelBookingId, cancellationToken);

        return new HotelBookingOperationalRead(
            booking.Id.Value,
            booking.Place.PlaceId,
            new DateOnly(booking.CheckInDate.Year, booking.CheckInDate.Month, booking.CheckInDate.Day),
            new DateOnly(booking.CheckOutDate.Year, booking.CheckOutDate.Month, booking.CheckOutDate.Day),
            new HotelBookingOperationalOccupancyRead(booking.RoomCount, booking.AdultCount, booking.ChildCount),
            booking.Status.ToString(),
            offer?.Monetary?.Total.Amount,
            offer?.Monetary?.Total.Currency.Value,
            hold?.Status.ToString(),
            reservation?.Status.ToString(),
            reservation?.Attempts.Count ?? 0,
            payment?.PaymentStatus,
            cancellation is { Status: HotelBookingCancellationStatus.RefundPending } ? "Pending" : null,
            cancellation?.Status.ToString(),
            issues.Count == 0 ? null : string.Join(", ", issues),
            reservation?.SourceKey ?? hold?.SourceKey,
            reservation?.SourceReservationReference);
    }

    public async Task<string> RecheckAvailabilityHoldAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        if (holdId == Guid.Empty)
        {
            return "NotFound";
        }

        var id = HotelAvailabilityHoldId.From(holdId);
        var hold = await _db.HotelAvailabilityHolds
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (hold is null)
        {
            return "NotFound";
        }

        if (_availability.Resolve(new AvailabilitySourceKey(hold.SourceKey)) is null)
        {
            return "CapabilityUnavailable";
        }

        var updated = await _holds.RecheckAsync(id, cancellationToken);
        return updated.Status.ToString();
    }

    public async Task<string> RecheckSupplierReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        if (reservationId == Guid.Empty)
        {
            return "NotFound";
        }

        var id = HotelSupplierReservationId.From(reservationId);
        var reservation = await _db.HotelSupplierReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (reservation is null)
        {
            return "NotFound";
        }

        if (_reservations.Resolve(new ReservationSourceKey(reservation.SourceKey)) is null)
        {
            return "CapabilityUnavailable";
        }

        var updated = await _reservationService.RecheckAsync(id, cancellationToken);
        return updated.Status.ToString();
    }

    public async Task<string> RecheckSupplierCancellationAsync(
        Guid cancellationId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationId == Guid.Empty)
        {
            return "NotFound";
        }

        var id = HotelBookingCancellationId.From(cancellationId);
        var cancellation = await _db.HotelBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cancellation is null)
        {
            return "NotFound";
        }

        var reservation = await _db.HotelSupplierReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.HotelBookingId == cancellation.HotelBookingId, cancellationToken);
        if (reservation is null || _reservations.Resolve(new ReservationSourceKey(reservation.SourceKey)) is null)
        {
            return "CapabilityUnavailable";
        }

        var updated = await _cancellations.RecheckAsync(id, cancellationToken);
        return updated.Status.ToString();
    }
}
