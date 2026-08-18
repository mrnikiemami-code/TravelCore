using NodaTime;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

internal sealed record PublicHotelBookingFacts(
    Stay Booking,
    HotelAvailabilityHold? Hold,
    HotelRateOfferSnapshot? Offer,
    HotelSupplierReservation? Reservation,
    HotelBookingCancellation? Cancellation,
    IReadOnlyList<HotelBookingReconciliationIssue> Issues,
    PublicPaymentRead? Payment,
    Instant Now);

internal static class PublicHotelBookingMapper
{
    public static PublicHotelBookingInitiationResponse ToInitiation(Stay booking, string? rawAccessToken)
    {
        return new PublicHotelBookingInitiationResponse(
            booking.Id.Value,
            booking.Status.ToString(),
            PublicHotelBookingPresentationStates.NeedsAvailability,
            rawAccessToken,
            AccessTokenIssued: rawAccessToken is not null,
            Confirmed: booking.Status == HotelBookingStatus.Confirmed,
            booking.Place.PlaceId,
            ToDateOnly(booking.CheckInDate),
            ToDateOnly(booking.CheckOutDate),
            MapRooms(booking));
    }

    public static PublicHotelBookingRead ToRead(PublicHotelBookingFacts facts)
    {
        var monetary = MapMonetary(facts.Offer, facts.Now);
        var cancellation = MapCancellation(facts.Cancellation);
        var paymentStatus = facts.Payment?.PaymentStatus;
        var refundStatus = facts.Payment?.RefundStatus;
        var presentation = DerivePresentation(facts, monetary);
        var cancellationAvailable = facts.Booking.Status == HotelBookingStatus.Confirmed
            && facts.Cancellation is null
            && monetary is not null
            && monetary.CancellationTerms.Any(term => term.CurrentlyExecutable);
        return new PublicHotelBookingRead(
            facts.Booking.Id.Value,
            facts.Booking.Status.ToString(),
            presentation,
            facts.Booking.Status == HotelBookingStatus.Confirmed,
            facts.Booking.Place.PlaceId,
            ToDateOnly(facts.Booking.CheckInDate),
            ToDateOnly(facts.Booking.CheckOutDate),
            new PublicHotelBookingContactRead(facts.Booking.Contact.Email, facts.Booking.Contact.Phone),
            MapRooms(facts.Booking),
            monetary,
            MapHold(facts.Hold),
            MapReservation(facts.Reservation),
            cancellation,
            paymentStatus,
            refundStatus,
            cancellationAvailable,
            monetary?.OfferExpired == true,
            SafeMessage(presentation));
    }

    public static PublicHotelBookingPaymentRead ToPayment(
        PublicHotelBookingRead booking,
        PublicPaymentRead? payment) =>
        new(
            booking.HotelBookingId,
            booking.Status,
            booking.Confirmed,
            booking.PresentationState,
            payment?.PaymentId,
            payment?.PaymentStatus ?? booking.PaymentStatus,
            payment?.Amount ?? booking.Monetary?.TotalAmount,
            payment?.CurrencyCode ?? booking.Monetary?.CurrencyCode,
            payment?.ProviderInitiationPossible == true
                && booking.Monetary is { OfferExpired: false }
                && string.Equals(booking.Status, nameof(HotelBookingStatus.Pending), StringComparison.Ordinal),
            payment?.LatestAttemptStatus,
            payment?.RefundStatus ?? booking.RefundStatus,
            payment?.SafeAction ?? "Unavailable",
            payment?.RedirectUri,
            booking.Monetary);

    private static string DerivePresentation(PublicHotelBookingFacts facts, PublicHotelBookingMonetaryRead? monetary)
    {
        if (facts.Issues.Any(issue => issue.BlocksConfirmation)
            && facts.Booking.Status == HotelBookingStatus.Pending
            && string.Equals(facts.Payment?.PaymentStatus, "Succeeded", StringComparison.Ordinal))
        {
            return PublicHotelBookingPresentationStates.ReconciliationRequired;
        }

        if (facts.Booking.Status == HotelBookingStatus.Cancelled)
        {
            if (string.Equals(facts.Payment?.RefundStatus, "Pending", StringComparison.Ordinal)
                || facts.Cancellation?.Status == HotelBookingCancellationStatus.RefundPending)
            {
                return PublicHotelBookingPresentationStates.RefundPending;
            }

            return PublicHotelBookingPresentationStates.Cancelled;
        }

        if (facts.Booking.Status == HotelBookingStatus.Confirmed)
        {
            if (facts.Cancellation is { Status: HotelBookingCancellationStatus.RefundPending })
            {
                return PublicHotelBookingPresentationStates.RefundPending;
            }

            if (facts.Cancellation is
                {
                    Status: HotelBookingCancellationStatus.Requested
                    or HotelBookingCancellationStatus.SupplierCancellationPending
                })
            {
                return PublicHotelBookingPresentationStates.CancellationPending;
            }

            if (facts.Cancellation is null
                && monetary is not null
                && monetary.CancellationTerms.Any(term => term.CurrentlyExecutable))
            {
                return PublicHotelBookingPresentationStates.CancellationAvailable;
            }

            return PublicHotelBookingPresentationStates.Confirmed;
        }

        if (string.Equals(facts.Payment?.PaymentStatus, "Succeeded", StringComparison.Ordinal))
        {
            if (facts.Reservation is
                {
                    Status: HotelSupplierReservationStatus.Pending
                })
            {
                return PublicHotelBookingPresentationStates.SupplierReservationPending;
            }

            return PublicHotelBookingPresentationStates.PaymentReceived;
        }

        if (facts.Payment is { LatestAttemptStatus: "Created" or "Initiated" })
        {
            return PublicHotelBookingPresentationStates.PaymentPending;
        }

        if (monetary is { OfferExpired: true })
        {
            return PublicHotelBookingPresentationStates.PaymentUnavailable;
        }

        if (monetary is not null)
        {
            if (facts.Payment is { ProviderInitiationPossible: false })
            {
                return PublicHotelBookingPresentationStates.PaymentUnavailable;
            }

            if (facts.Hold is { Status: HotelAvailabilityHoldStatus.Active })
            {
                return PublicHotelBookingPresentationStates.ReadyForPayment;
            }

            return PublicHotelBookingPresentationStates.RateAccepted;
        }

        if (facts.Hold is { Status: HotelAvailabilityHoldStatus.Active })
        {
            return PublicHotelBookingPresentationStates.NeedsRate;
        }

        if (facts.Hold is { Status: HotelAvailabilityHoldStatus.Requested })
        {
            return PublicHotelBookingPresentationStates.AvailabilityPending;
        }

        return PublicHotelBookingPresentationStates.NeedsAvailability;
    }

    private static string SafeMessage(string presentation) =>
        presentation switch
        {
            PublicHotelBookingPresentationStates.PaymentReceived
                or PublicHotelBookingPresentationStates.SupplierReservationPending =>
                "Payment received; hotel confirmation is being processed.",
            PublicHotelBookingPresentationStates.ReconciliationRequired =>
                "We're checking your reservation.",
            PublicHotelBookingPresentationStates.CancellationPending =>
                "Cancellation is being processed.",
            PublicHotelBookingPresentationStates.RefundPending =>
                "A money return is in progress.",
            PublicHotelBookingPresentationStates.PaymentUnavailable =>
                "Online payment is not currently available.",
            PublicHotelBookingPresentationStates.NeedsAvailability =>
                "Availability is not confirmed. This is not a confirmed booking.",
            _ => "This page does not confirm payment or hotel reservation by itself.",
        };

    private static IReadOnlyList<PublicHotelBookingRoomRead> MapRooms(Stay booking) =>
        booking.Rooms
            .OrderBy(room => room.Ordinal)
            .Select(room => new PublicHotelBookingRoomRead(
                room.Id.Value,
                room.Ordinal,
                room.Guests.Select(guest => new PublicHotelBookingGuestRead(
                    guest.Id.Value,
                    guest.GivenName,
                    guest.FamilyName,
                    guest.Category.ToString(),
                    guest.AgeAtCheckIn?.Years,
                    guest.IsLeadGuest)).ToList()))
            .ToList();

    private static PublicHotelBookingHoldRead? MapHold(HotelAvailabilityHold? hold) =>
        hold is null
            ? null
            : new PublicHotelBookingHoldRead(
                hold.Status.ToString(),
                hold.ExpiresAt?.ToDateTimeOffset());

    private static PublicHotelBookingReservationRead? MapReservation(HotelSupplierReservation? reservation)
    {
        if (reservation is null)
        {
            return null;
        }

        return new PublicHotelBookingReservationRead(
            reservation.Status.ToString(),
            reservation.Status == HotelSupplierReservationStatus.Confirmed
                ? reservation.SupplierConfirmationCode
                : null);
    }

    private static PublicHotelBookingCancellationRead? MapCancellation(HotelBookingCancellation? cancellation)
    {
        if (cancellation is null)
        {
            return null;
        }

        return new PublicHotelBookingCancellationRead(
            cancellation.Status.ToString(),
            cancellation.FinancialOutcome.ToString(),
            cancellation.PenaltyAmount,
            cancellation.RefundAmount,
            cancellation.CurrencyCode);
    }

    private static PublicHotelBookingMonetaryRead? MapMonetary(HotelRateOfferSnapshot? offer, Instant now)
    {
        if (offer?.Monetary is null)
        {
            return null;
        }

        var expired = offer.OfferExpiresAt is { } expires && expires <= now;
        var total = offer.Monetary.Total;
        var terms = offer.CancellationPolicy.Rules
            .OrderBy(rule => rule.Ordinal)
            .Select(rule =>
            {
                var executable = rule.Penalty.Amount == 0m || rule.Penalty.Equals(total);
                return new PublicHotelBookingCancellationTermRead(
                    rule.EffectiveFrom.ToDateTimeOffset(),
                    rule.EffectiveUntil?.ToDateTimeOffset(),
                    rule.Penalty.Amount,
                    rule.Penalty.Currency.Value,
                    executable && !expired);
            })
            .ToList();

        return new PublicHotelBookingMonetaryRead(
            offer.Id.Value,
            total.Currency.Value,
            total.Amount,
            offer.OfferExpiresAt?.ToDateTimeOffset(),
            expired,
            terms,
            offer.CancellationPolicy.PublicExplanation);
    }

    private static DateOnly ToDateOnly(LocalDate date) => new(date.Year, date.Month, date.Day);
}
