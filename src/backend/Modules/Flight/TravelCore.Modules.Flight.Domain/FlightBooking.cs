using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight-owned live-flight transaction aggregate (P22-R2 / P22-R6).
/// Status is independent of PNR, Payment, and tickets until all three confirm.
/// </summary>
public sealed class FlightBooking
{
    private readonly List<FlightJourney> _journeys = [];
    private readonly List<FlightPassenger> _passengers = [];

    private FlightBooking()
    {
    }

    private FlightBooking(FlightBookingId id, FlightTripType tripType)
    {
        Id = id;
        TripType = tripType;
        Status = FlightBookingStatus.Pending;
        Version = 0;
    }

    public FlightBookingId Id { get; private set; }

    public FlightTripType TripType { get; private set; }

    public FlightBookingStatus Status { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? CancelledAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<FlightJourney> Journeys => _journeys;

    public IReadOnlyList<FlightPassenger> Passengers => _passengers;

    public int JourneyCount => _journeys.Count;

    public int PassengerCount => _passengers.Count;

    public FlightJourney Outbound => _journeys.OrderBy(j => j.Ordinal).First();

    public FlightJourney? ReturnJourney =>
        TripType == FlightTripType.RoundTrip ? _journeys.OrderBy(j => j.Ordinal).Skip(1).First() : null;

    public static FlightBooking Create(
        FlightTripType tripType,
        IReadOnlyList<FlightJourneySpecification> journeys,
        IReadOnlyList<FlightPassengerSpecification> passengers)
    {
        if (!Enum.IsDefined(tripType))
        {
            throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled.");
        }

        ArgumentNullException.ThrowIfNull(journeys);
        ArgumentNullException.ThrowIfNull(passengers);

        var expectedJourneys = tripType switch
        {
            FlightTripType.OneWay => 1,
            FlightTripType.RoundTrip => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled."),
        };

        if (journeys.Count != expectedJourneys)
        {
            throw new ArgumentException(
                $"{tripType} FlightBooking requires exactly {expectedJourneys} journey(s).",
                nameof(journeys));
        }

        if (passengers.Count == 0)
        {
            throw new ArgumentException("FlightBooking requires at least one passenger.", nameof(passengers));
        }

        var booking = new FlightBooking(FlightBookingId.New(), tripType);
        var journeyOrdinal = 1;
        foreach (var spec in journeys)
        {
            ArgumentNullException.ThrowIfNull(spec);
            booking._journeys.Add(FlightJourney.Create(booking.Id, journeyOrdinal, spec));
            journeyOrdinal++;
        }

        if (tripType == FlightTripType.RoundTrip)
        {
            var outbound = booking._journeys[0];
            var inbound = booking._journeys[1];
            if (inbound.Origin.IataCode != outbound.Destination.IataCode
                || inbound.Destination.IataCode != outbound.Origin.IataCode)
            {
                throw new ArgumentException(
                    "RoundTrip return origin/destination must reverse the outbound journey.",
                    nameof(journeys));
            }
        }

        var passengerOrdinal = 1;
        foreach (var spec in passengers)
        {
            ArgumentNullException.ThrowIfNull(spec);
            booking._passengers.Add(FlightPassenger.Create(booking.Id, passengerOrdinal, spec));
            passengerOrdinal++;
        }

        if (!booking._passengers.Any(p => p.Category == FlightPassengerCategory.Adult))
        {
            throw new ArgumentException("FlightBooking requires at least one Adult passenger.", nameof(passengers));
        }

        return booking;
    }

    public void EnsureMatchesCommercialOffer(
        FlightTripType tripType,
        IReadOnlyList<FlightOfferSegmentIdentity> segments,
        FlightPassengerCount passengers)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(passengers);

        if (tripType != TripType)
        {
            throw new ArgumentException("Offer trip type does not match persisted FlightBooking.", nameof(tripType));
        }

        var expected = _journeys
            .OrderBy(j => j.Ordinal)
            .SelectMany(j => j.Segments
                .OrderBy(s => s.Ordinal)
                .Select(s => new FlightOfferSegmentIdentity(
                    j.Ordinal,
                    s.Ordinal,
                    s.Origin,
                    s.Destination,
                    s.DepartureAt,
                    s.ArrivalAt,
                    s.MarketingCarrier,
                    s.OperatingCarrier,
                    s.FlightNumber)))
            .ToArray();

        if (segments.Count == 0 || segments.Count != expected.Length)
        {
            throw new ArgumentException(
                "Accepted offer must cover every FlightSegment of the persisted FlightBooking exactly once.",
                nameof(segments));
        }

        var offered = segments.OrderBy(s => s.JourneyOrdinal).ThenBy(s => s.SegmentOrdinal).ToArray();
        for (var i = 0; i < expected.Length; i++)
        {
            if (!expected[i].Equals(offered[i]))
            {
                throw new ArgumentException(
                    "Offer itinerary identity does not match persisted FlightBooking.",
                    nameof(segments));
            }
        }

        var adultCount = _passengers.Count(p => p.Category == FlightPassengerCategory.Adult);
        var childCount = _passengers.Count(p => p.Category == FlightPassengerCategory.Child);
        var infantCount = _passengers.Count(p => p.Category == FlightPassengerCategory.Infant);
        if (passengers.AdultCount != adultCount
            || passengers.ChildCount != childCount
            || passengers.InfantCount != infantCount)
        {
            throw new ArgumentException(
                "Offer passenger composition does not match persisted FlightBooking.",
                nameof(passengers));
        }
    }

    /// <summary>
    /// Triple-evidence confirmation: Confirmed reservation + Payment Succeeded + all passenger tickets Issued.
    /// Not a generic Confirm/SetConfirmed surface. PNR or Payment alone cannot confirm.
    /// </summary>
    public void ConfirmFromAuthoritativeReservationPaymentAndTickets(
        FlightSupplierReservation reservation,
        FlightBookingPaymentEvidence paymentEvidence,
        IReadOnlyList<FlightTicket> tickets,
        FlightBookingMonetarySnapshot monetary,
        IReadOnlyList<FlightReconciliationIssue> existingIssues,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(paymentEvidence);
        ArgumentNullException.ThrowIfNull(tickets);
        ArgumentNullException.ThrowIfNull(monetary);
        ArgumentNullException.ThrowIfNull(existingIssues);
        EnsureClock(now);

        if (Status == FlightBookingStatus.Confirmed)
        {
            return;
        }

        if (Status == FlightBookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled FlightBooking cannot become Confirmed.");
        }

        if (Status != FlightBookingStatus.Pending)
        {
            throw new InvalidOperationException($"FlightBooking in status {Status} cannot become Confirmed.");
        }

        if (!reservation.FlightBookingId.Equals(Id)
            || reservation.Status != FlightSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "FlightBooking confirmation requires an authoritatively Confirmed FlightSupplierReservation.");
        }

        if (!paymentEvidence.FlightBookingId.Equals(Id)
            || !paymentEvidence.MatchesMonetarySnapshot(monetary))
        {
            throw new InvalidOperationException(
                "Payment evidence amount/currency does not match FlightBookingMonetarySnapshot.");
        }

        if (existingIssues.Any(issue => issue.FlightBookingId.Equals(Id) && issue.BlocksConfirmation))
        {
            throw new InvalidOperationException(
                "Blocking Flight reconciliation evidence prevents confirmation.");
        }

        var passengerIds = _passengers.Select(p => p.Id).ToHashSet();
        if (passengerIds.Count == 0)
        {
            throw new InvalidOperationException("FlightBooking confirmation requires passengers.");
        }

        var issuedForBooking = tickets
            .Where(t => t.FlightBookingId.Equals(Id) && t.Status == FlightTicketStatus.Issued)
            .Select(t => t.PassengerId)
            .ToHashSet();
        if (!passengerIds.SetEquals(issuedForBooking))
        {
            throw new InvalidOperationException(
                "FlightBooking confirmation requires an Issued ticket for every passenger.");
        }

        Status = FlightBookingStatus.Confirmed;
        ConfirmedAt = now;
        IncrementVersion();
    }

    /// <summary>
    /// System compensation terminalization: Pending → Cancelled after authoritative full Refund.
    /// Does not cancel Confirmed FlightBooking (R7). Not a generic Cancel/SetCancelled surface.
    /// </summary>
    public void CancelFromAuthoritativePaymentCompensation(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightBookingStatus.Cancelled)
        {
            return;
        }

        if (Status == FlightBookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Confirmed FlightBooking cannot be cancelled by payment compensation.");
        }

        if (Status != FlightBookingStatus.Pending)
        {
            throw new InvalidOperationException($"FlightBooking in status {Status} cannot become Cancelled.");
        }

        Status = FlightBookingStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    /// <summary>
    /// Confirmed → Cancelled only after authoritative reservation Cancelled and every passenger ticket is Voided or Refunded.
    /// Not a generic Cancel/SetCancelled/ForceCancel surface. Distinct from R6 payment compensation.
    /// </summary>
    public void CancelFromAuthoritativeSupplierReversal(
        FlightSupplierReservation reservation,
        IReadOnlyList<FlightTicket> tickets,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(tickets);
        EnsureClock(now);

        if (Status == FlightBookingStatus.Cancelled)
        {
            return;
        }

        if (Status != FlightBookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                $"FlightBooking in status {Status} cannot be cancelled from supplier reversal.");
        }

        if (!reservation.FlightBookingId.Equals(Id)
            || reservation.Status != FlightSupplierReservationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "FlightBooking cancellation requires an authoritatively Cancelled FlightSupplierReservation.");
        }

        var passengerIds = _passengers.Select(p => p.Id).ToHashSet();
        if (passengerIds.Count == 0)
        {
            throw new InvalidOperationException("FlightBooking cancellation requires passengers.");
        }

        var reversedForBooking = tickets
            .Where(t => t.FlightBookingId.Equals(Id)
                && t.Status is FlightTicketStatus.Voided or FlightTicketStatus.Refunded)
            .Select(t => t.PassengerId)
            .ToHashSet();
        if (!passengerIds.SetEquals(reversedForBooking))
        {
            throw new InvalidOperationException(
                "FlightBooking cannot become Cancelled while required passenger tickets remain active.");
        }

        Status = FlightBookingStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    private void IncrementVersion() => Version++;

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
