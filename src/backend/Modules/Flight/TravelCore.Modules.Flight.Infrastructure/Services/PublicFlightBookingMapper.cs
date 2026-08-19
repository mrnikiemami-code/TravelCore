using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

internal sealed record PublicFlightBookingFacts(
    FlightBookingAggregate Booking,
    FlightOfferSnapshot? Offer,
    FlightSupplierReservation? Reservation,
    IReadOnlyList<FlightTicket> Tickets,
    FlightBookingCancellation? Cancellation,
    IReadOnlyList<FlightReconciliationIssue> Issues,
    PublicPaymentRead? Payment,
    Instant Now);

internal static class PublicFlightBookingMapper
{
    public static PublicFlightBookingInitiationResponse ToInitiation(
        FlightBookingAggregate booking,
        string? rawAccessToken) =>
        new(
            booking.Id.Value,
            booking.Status.ToString(),
            PublicFlightBookingPresentationStates.NeedsOffer,
            rawAccessToken,
            AccessTokenIssued: rawAccessToken is not null,
            Confirmed: booking.Status == FlightBookingStatus.Confirmed,
            booking.TripType.ToString(),
            MapJourneys(booking),
            MapPassengers(booking));

    public static PublicFlightSearchResultRead ToSearch(FlightSearchResult result, bool sourceConfigured)
    {
        var options = result.Options.Select(MapSearchOption).ToList();
        var message = sourceConfigured
            ? result.Completion == FlightSearchCompletion.Unknown
                ? "Flight search timed out. No options were fabricated."
                : options.Count == 0
                    ? "No flights matched this search."
                    : null
            : "Flight search is not currently available.";
        return new PublicFlightSearchResultRead(
            result.Completion.ToString(),
            sourceConfigured,
            message,
            options);
    }

    public static PublicFlightBookingRead ToRead(PublicFlightBookingFacts facts)
    {
        var offer = MapOffer(facts.Offer, facts.Now);
        var tickets = MapTickets(facts.Tickets);
        var cancellation = MapCancellation(facts.Cancellation);
        var presentation = DerivePresentation(facts, offer, tickets);
        var cancellationAvailable = facts.Booking.Status == FlightBookingStatus.Confirmed
            && facts.Cancellation is null
            && offer is { OfferExpired: false };
        return new PublicFlightBookingRead(
            facts.Booking.Id.Value,
            facts.Booking.Status.ToString(),
            presentation,
            facts.Booking.Status == FlightBookingStatus.Confirmed,
            facts.Booking.TripType.ToString(),
            MapJourneys(facts.Booking),
            MapPassengers(facts.Booking),
            offer,
            MapReservation(facts.Reservation),
            tickets,
            cancellation,
            facts.Payment?.PaymentStatus,
            facts.Payment?.RefundStatus,
            cancellationAvailable,
            offer?.OfferExpired == true,
            SafeMessage(presentation));
    }

    public static PublicFlightBookingPaymentRead ToPayment(
        PublicFlightBookingRead booking,
        PublicPaymentRead? payment) =>
        new(
            booking.FlightBookingId,
            booking.Status,
            booking.Confirmed,
            booking.PresentationState,
            payment?.PaymentId,
            payment?.PaymentStatus ?? booking.PaymentStatus,
            payment?.Amount ?? booking.Offer?.TotalAmount,
            payment?.CurrencyCode ?? booking.Offer?.CurrencyCode,
            payment?.ProviderInitiationPossible == true
                && booking.Offer is { OfferExpired: false }
                && string.Equals(booking.Status, nameof(FlightBookingStatus.Pending), StringComparison.Ordinal)
                && booking.Reservation?.PresentationStatus
                    == PublicFlightBookingPresentationStates.ReservationConfirmed,
            payment?.LatestAttemptStatus,
            payment?.RefundStatus ?? booking.RefundStatus,
            payment?.SafeAction ?? "Unavailable",
            payment?.RedirectUri,
            booking.Offer);

    internal static PublicFlightBookingJourneyStatus MapOfferException(InvalidOperationException ex)
    {
        var message = ex.Message;
        if (message.Contains("unconfigured", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot be fabricated", StringComparison.OrdinalIgnoreCase))
        {
            return PublicFlightBookingJourneyStatus.SourceUnavailable;
        }

        if (message.Contains("Expired", StringComparison.OrdinalIgnoreCase))
        {
            return PublicFlightBookingJourneyStatus.OfferExpired;
        }

        if (message.Contains("requote", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Silent repricing", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Changed", StringComparison.OrdinalIgnoreCase))
        {
            return PublicFlightBookingJourneyStatus.OfferRequoteRequired;
        }

        if (message.Contains("Unavailable", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return PublicFlightBookingJourneyStatus.OfferUnavailable;
        }

        return PublicFlightBookingJourneyStatus.Ineligible;
    }

    private static string DerivePresentation(
        PublicFlightBookingFacts facts,
        PublicFlightOfferRead? offer,
        IReadOnlyList<PublicFlightTicketRead> tickets)
    {
        if (facts.Issues.Any(issue => issue.BlocksConfirmation)
            && facts.Booking.Status == FlightBookingStatus.Pending
            && string.Equals(facts.Payment?.PaymentStatus, "Succeeded", StringComparison.Ordinal))
        {
            return PublicFlightBookingPresentationStates.ReconciliationRequired;
        }

        if (facts.Booking.Status == FlightBookingStatus.Cancelled)
        {
            if (string.Equals(facts.Payment?.RefundStatus, "Pending", StringComparison.Ordinal)
                || facts.Cancellation?.Status == FlightBookingCancellationStatus.RefundPending)
            {
                return PublicFlightBookingPresentationStates.RefundPending;
            }

            return PublicFlightBookingPresentationStates.Cancelled;
        }

        if (facts.Booking.Status == FlightBookingStatus.Confirmed)
        {
            if (facts.Cancellation is { Status: FlightBookingCancellationStatus.RefundPending })
            {
                return PublicFlightBookingPresentationStates.RefundPending;
            }

            if (facts.Cancellation is
                {
                    Status: FlightBookingCancellationStatus.Requested
                    or FlightBookingCancellationStatus.SupplierReversalPending
                })
            {
                return PublicFlightBookingPresentationStates.CancellationPending;
            }

            if (facts.Cancellation is null && offer is { OfferExpired: false })
            {
                return PublicFlightBookingPresentationStates.CancellationAvailable;
            }

            return PublicFlightBookingPresentationStates.Confirmed;
        }

        if (string.Equals(facts.Payment?.PaymentStatus, "Succeeded", StringComparison.Ordinal))
        {
            var issuedCount = tickets.Count(t => t.Status == nameof(FlightTicketStatus.Issued));
            if (tickets.Count > 0 && issuedCount > 0 && issuedCount < tickets.Count)
            {
                return PublicFlightBookingPresentationStates.TicketingPending;
            }

            if (tickets.Any(t => t.Status == nameof(FlightTicketStatus.Pending))
                || facts.Reservation is { Status: FlightSupplierReservationStatus.Confirmed })
            {
                return PublicFlightBookingPresentationStates.TicketingPending;
            }

            return PublicFlightBookingPresentationStates.PaymentReceived;
        }

        if (facts.Payment is { LatestAttemptStatus: "Created" or "Initiated" })
        {
            return PublicFlightBookingPresentationStates.PaymentPending;
        }

        if (facts.Reservation is { Status: FlightSupplierReservationStatus.Expired })
        {
            return PublicFlightBookingPresentationStates.ReservationExpired;
        }

        if (facts.Reservation is { Status: FlightSupplierReservationStatus.Confirmed })
        {
            if (offer is { OfferExpired: true })
            {
                return PublicFlightBookingPresentationStates.PaymentUnavailable;
            }

            if (facts.Payment is { ProviderInitiationPossible: false } || offer is not null)
            {
                if (facts.Payment is { ProviderInitiationPossible: false })
                {
                    return PublicFlightBookingPresentationStates.PaymentUnavailable;
                }

                return PublicFlightBookingPresentationStates.ReadyForPayment;
            }

            return PublicFlightBookingPresentationStates.ReservationConfirmed;
        }

        if (facts.Reservation is { Status: FlightSupplierReservationStatus.Pending })
        {
            return PublicFlightBookingPresentationStates.ReservationPending;
        }

        if (offer is { OfferExpired: true })
        {
            return PublicFlightBookingPresentationStates.OfferExpired;
        }

        if (offer is not null)
        {
            return PublicFlightBookingPresentationStates.OfferAccepted;
        }

        return PublicFlightBookingPresentationStates.NeedsOffer;
    }

    private static string SafeMessage(string presentation) =>
        presentation switch
        {
            PublicFlightBookingPresentationStates.PaymentReceived
                or PublicFlightBookingPresentationStates.TicketingPending =>
                "Payment received; tickets are being processed.",
            PublicFlightBookingPresentationStates.ReconciliationRequired =>
                "We're checking your tickets and reservation.",
            PublicFlightBookingPresentationStates.CancellationPending =>
                "Cancellation is being processed.",
            PublicFlightBookingPresentationStates.RefundPending =>
                "A money return is in progress.",
            PublicFlightBookingPresentationStates.PaymentUnavailable =>
                "Online payment is not currently available.",
            PublicFlightBookingPresentationStates.OfferExpired =>
                "This offer has expired. A new quote is required.",
            PublicFlightBookingPresentationStates.OfferRequoteRequired =>
                "The fare changed. A new quote is required.",
            PublicFlightBookingPresentationStates.ReservationPending =>
                "Reservation is being processed. This is not a confirmed booking.",
            PublicFlightBookingPresentationStates.ReservationExpired =>
                "The reservation expired. This is not a confirmed booking.",
            PublicFlightBookingPresentationStates.NeedsOffer =>
                "An offer has not been accepted. This is not a confirmed booking.",
            _ => "This page does not confirm payment, reservation, or tickets by itself.",
        };

    private static PublicFlightSearchOptionRead MapSearchOption(FlightSearchOption option) =>
        new(
            option.SourceOptionReference,
            option.TripType.ToString(),
            option.Journeys.Select(journey => new PublicFlightSearchJourneyRead(
                journey.Ordinal,
                journey.Segments.Select(segment => new PublicFlightSearchSegmentRead(
                    segment.Ordinal,
                    segment.Origin.IataCode,
                    segment.Destination.IataCode,
                    segment.DepartureAt.ToDateTimeOffset(),
                    segment.DepartureTimeZoneId,
                    segment.ArrivalAt.ToDateTimeOffset(),
                    segment.ArrivalTimeZoneId,
                    segment.MarketingCarrier.IataCode,
                    segment.OperatingCarrier?.IataCode,
                    segment.FlightNumber)).ToList())).ToList(),
            option.ObservedAt.ToDateTimeOffset(),
            option.ExpiresAt?.ToDateTimeOffset());

    private static IReadOnlyList<PublicFlightJourneyRead> MapJourneys(FlightBookingAggregate booking) =>
        booking.Journeys
            .OrderBy(j => j.Ordinal)
            .Select(journey => new PublicFlightJourneyRead(
                journey.Id.Value,
                journey.Ordinal,
                journey.Segments
                    .OrderBy(s => s.Ordinal)
                    .Select(segment => new PublicFlightSegmentRead(
                        segment.Id.Value,
                        segment.Ordinal,
                        segment.Origin.IataCode,
                        segment.Destination.IataCode,
                        segment.DepartureAt.ToDateTimeOffset(),
                        segment.DepartureTimeZoneId,
                        segment.ArrivalAt.ToDateTimeOffset(),
                        segment.ArrivalTimeZoneId,
                        segment.MarketingCarrier.IataCode,
                        segment.OperatingCarrier?.IataCode,
                        segment.FlightNumber)).ToList()))
            .ToList();

    private static IReadOnlyList<PublicFlightPassengerRead> MapPassengers(FlightBookingAggregate booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new PublicFlightPassengerRead(
                p.Id.Value,
                p.GivenName,
                p.FamilyName,
                p.Category.ToString()))
            .ToList();

    private static PublicFlightOfferRead? MapOffer(FlightOfferSnapshot? offer, Instant now)
    {
        if (offer?.Monetary is null)
        {
            return null;
        }

        var expired = offer.OfferExpiresAt <= now;
        var rules = offer.FareRules;
        return new PublicFlightOfferRead(
            offer.Id.Value,
            offer.Monetary.Total.Currency.Value,
            offer.Monetary.Total.Amount,
            offer.OfferExpiresAt.ToDateTimeOffset(),
            expired,
            rules.TicketingDeadline?.ToDateTimeOffset(),
            new PublicFlightFareRulesRead(
                rules.Refundable,
                rules.Changeable,
                rules.TicketingDeadline?.ToDateTimeOffset(),
                rules.CancelPenalty?.Amount,
                rules.CancelPenalty?.Currency.Value,
                rules.Baggage.Select(b => new PublicFlightBaggageRead(
                    b.Quantity,
                    b.Weight,
                    b.Unit,
                    b.Category,
                    b.PassengerCategory?.ToString())).ToList()));
    }

    private static PublicFlightReservationRead? MapReservation(FlightSupplierReservation? reservation)
    {
        if (reservation is null)
        {
            return null;
        }

        var presentation = reservation.Status switch
        {
            FlightSupplierReservationStatus.Confirmed => PublicFlightBookingPresentationStates.ReservationConfirmed,
            FlightSupplierReservationStatus.Expired => PublicFlightBookingPresentationStates.ReservationExpired,
            _ => PublicFlightBookingPresentationStates.ReservationPending,
        };
        return new PublicFlightReservationRead(
            presentation,
            reservation.Status == FlightSupplierReservationStatus.Confirmed
                ? reservation.ReservationLocator
                : null,
            reservation.ReservationExpiresAt?.ToDateTimeOffset());
    }

    private static IReadOnlyList<PublicFlightTicketRead> MapTickets(IReadOnlyList<FlightTicket> tickets) =>
        tickets
            .Select(ticket => new PublicFlightTicketRead(
                ticket.PassengerId.Value,
                ticket.Status == FlightTicketStatus.Issued ? nameof(FlightTicketStatus.Issued) : "Pending",
                ticket.Status == FlightTicketStatus.Issued ? ticket.SourceTicketNumber : null))
            .ToList();

    private static PublicFlightCancellationRead? MapCancellation(FlightBookingCancellation? cancellation)
    {
        if (cancellation is null)
        {
            return null;
        }

        return new PublicFlightCancellationRead(
            cancellation.Status.ToString(),
            cancellation.FinancialOutcome.ToString(),
            cancellation.PenaltyAmount,
            cancellation.RefundAmount,
            cancellation.CurrencyCode);
    }
}
