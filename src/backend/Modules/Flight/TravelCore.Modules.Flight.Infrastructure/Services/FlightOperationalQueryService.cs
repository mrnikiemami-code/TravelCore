using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// Internal read-only operational FlightBooking query (P22-R8).
/// Does not mutate booking/financial truth. Redacts passenger PII beyond counts, tokens, and payloads.
/// </summary>
internal sealed class FlightOperationalQueryService : IFlightOperationalQuery
{
    private readonly FlightDbContext _db;
    private readonly IFlightReservationSourceResolver _reservations;
    private readonly IFlightCancellationSourceResolver _cancellationsResolver;
    private readonly FlightSupplierReservationService _reservationService;
    private readonly FlightBookingCancellationService _cancellations;
    private readonly IPaymentSuccessEvidenceQuery _paymentEvidence;

    public FlightOperationalQueryService(
        FlightDbContext db,
        IFlightReservationSourceResolver reservations,
        IFlightCancellationSourceResolver cancellationsResolver,
        FlightSupplierReservationService reservationService,
        FlightBookingCancellationService cancellations,
        IPaymentSuccessEvidenceQuery paymentEvidence)
    {
        _db = db;
        _reservations = reservations;
        _cancellationsResolver = cancellationsResolver;
        _reservationService = reservationService;
        _cancellations = cancellations;
        _paymentEvidence = paymentEvidence;
    }

    public async Task<FlightOperationalRead?> GetByFlightBookingIdAsync(
        Guid flightBookingId,
        CancellationToken cancellationToken = default)
    {
        if (flightBookingId == Guid.Empty)
        {
            return null;
        }

        var id = FlightBookingId.From(flightBookingId);
        var booking = await _db.FlightBookings
            .AsNoTracking()
            .Include(x => x.Journeys)
            .ThenInclude(x => x.Segments)
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var offer = await _db.FlightOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .SingleOrDefaultAsync(x => x.FlightBookingId == id, cancellationToken);
        var reservation = await _db.FlightSupplierReservations
            .AsNoTracking()
            .Include(x => x.Attempts)
            .Where(x => x.FlightBookingId == id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var tickets = await _db.FlightTickets
            .AsNoTracking()
            .Where(x => x.FlightBookingId == id)
            .ToListAsync(cancellationToken);
        var cancellation = await _db.FlightBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FlightBookingId == id, cancellationToken);
        var issues = await _db.FlightReconciliationIssues
            .AsNoTracking()
            .Where(x => x.FlightBookingId == id)
            .Select(x => x.Kind.ToString())
            .ToListAsync(cancellationToken);
        var payment = await _paymentEvidence.GetByFlightBookingIdAsync(flightBookingId, cancellationToken);
        var outbound = booking.Outbound;
        var firstSegment = outbound.Segments.OrderBy(s => s.Ordinal).First();

        var issued = tickets.Count(t => t.Status == FlightTicketStatus.Issued);
        var ticketSummary = tickets.Count == 0
            ? null
            : $"{issued}/{tickets.Count} issued";

        var reservationPresentation = reservation?.Status switch
        {
            FlightSupplierReservationStatus.Confirmed => PublicFlightBookingPresentationStates.ReservationConfirmed,
            FlightSupplierReservationStatus.Expired => PublicFlightBookingPresentationStates.ReservationExpired,
            FlightSupplierReservationStatus.Pending => PublicFlightBookingPresentationStates.ReservationPending,
            _ => reservation?.Status.ToString(),
        };

        return new FlightOperationalRead(
            booking.Id.Value,
            booking.TripType.ToString(),
            outbound.Origin.IataCode,
            outbound.Destination.IataCode,
            firstSegment.DepartureAt.ToDateTimeOffset(),
            new FlightOperationalPassengerCountRead(
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant)),
            booking.Status.ToString(),
            offer?.Monetary?.Total.Amount,
            offer?.Monetary?.Total.Currency.Value,
            offer?.SourceKey,
            reservationPresentation,
            reservation?.Attempts.Count ?? 0,
            payment?.PaymentStatus,
            cancellation is { Status: FlightBookingCancellationStatus.RefundPending } ? "Pending" : null,
            ticketSummary,
            cancellation?.Status.ToString(),
            issues.Count == 0 ? null : string.Join(", ", issues),
            reservation?.SourceKey ?? offer?.SourceKey,
            reservation?.SourceReservationReference);
    }

    public async Task<string> RecheckSupplierReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        if (reservationId == Guid.Empty)
        {
            return "NotFound";
        }

        var id = FlightSupplierReservationId.From(reservationId);
        var reservation = await _db.FlightSupplierReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (reservation is null)
        {
            return "NotFound";
        }

        if (_reservations.Resolve(new FlightSourceKey(reservation.SourceKey)) is null)
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

        var id = FlightBookingCancellationId.From(cancellationId);
        var cancellation = await _db.FlightBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cancellation is null)
        {
            return "NotFound";
        }

        var reservation = await _db.FlightSupplierReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FlightBookingId == cancellation.FlightBookingId, cancellationToken);
        if (reservation is null
            || _cancellationsResolver.Resolve(new FlightSourceKey(reservation.SourceKey)) is null)
        {
            return "CapabilityUnavailable";
        }

        var updated = await _cancellations.RecheckAsync(id, cancellationToken);
        return updated.Status.ToString();
    }
}
