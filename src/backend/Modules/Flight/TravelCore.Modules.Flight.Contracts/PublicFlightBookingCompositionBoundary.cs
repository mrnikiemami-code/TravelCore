namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Public FlightBooking transactional journey (TC-P22-T008 / P22-R8).
/// Behavior-oriented, not generic CRUD. Independent of Tour and Hotel access tokens.
/// </summary>
public static class PublicFlightBookingCompositionBoundary
{
    public const string PublicApiGroup = "/api/flight-booking/public";
    public const string AccessTokenHeader = "X-TravelCore-Flight-Booking-Access-Token";
    public const string IdempotencyHeader = "Idempotency-Key";
    public const string ActorAccountIdClaimType = "tc_account_id";
    public const string PublicFlightBookingIsNotCrud = "Public FlightBooking != CRUD";
    public const string PublicInitiationIsNotConfirmation = "Public FlightBooking initiation != FlightBooking confirmation";
    public const string PendingIsNotConfirmed = "Pending != Confirmed";
    public const string FlightBookingIdIsNotAccessCredential = "FlightBookingId != Access Credential";
    public const string PaymentIdIsNotAccessCredential = "PaymentId != Access Credential";
    public const string ReservationLocatorIsNotAccessCredential = "ReservationLocator != Access Credential";
    public const string FlightTokenIsNotTourToken =
        "FlightBooking access token != Tour Booking access token";
    public const string FlightTokenIsNotHotelToken =
        "FlightBooking access token != HotelBooking access token";
    public const string PnrConfirmedIsNotFlightConfirmed =
        "PNR Confirmed != FlightBooking Confirmed";
    public const string PaymentSucceededIsNotFlightConfirmed =
        "Payment Succeeded != FlightBooking Confirmed";
    public const string TicketIssuedIsNotFlightConfirmed =
        "Ticket Issued != FlightBooking Confirmed";
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const bool PublicListingImplemented = false;
    public const bool GenericCrudImplemented = false;
    public const bool PublicRefundCommandImplemented = false;
    public const bool CardCollectionImplemented = false;
    public const bool OperationalHttpRouteImplemented = false;
    public const bool RawTokenUrlExposureImplemented = false;
    public const bool RawTokenLocalStorageImplemented = false;
    public const bool MultiCityImplemented = false;
}

public static class PublicFlightBookingPresentationStates
{
    public const string NeedsOffer = "NeedsOffer";
    public const string OfferExpired = "OfferExpired";
    public const string OfferRequoteRequired = "OfferRequoteRequired";
    public const string OfferAccepted = "OfferAccepted";
    public const string ReservationPending = "ReservationPending";
    public const string ReservationConfirmed = "ReservationConfirmed";
    public const string ReservationExpired = "ReservationExpired";
    public const string PaymentUnavailable = "PaymentUnavailable";
    public const string ReadyForPayment = "ReadyForPayment";
    public const string PaymentPending = "PaymentPending";
    public const string PaymentReceived = "PaymentReceived";
    public const string TicketingPending = "TicketingPending";
    public const string Confirmed = "Confirmed";
    public const string CancellationAvailable = "CancellationAvailable";
    public const string CancellationPending = "CancellationPending";
    public const string RefundPending = "RefundPending";
    public const string Cancelled = "Cancelled";
    public const string ReconciliationRequired = "ReconciliationRequired";
}
