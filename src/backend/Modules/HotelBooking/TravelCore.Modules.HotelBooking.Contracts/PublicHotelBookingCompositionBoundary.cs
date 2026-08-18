namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Public HotelBooking transactional journey (TC-P21-T008 / P21-R8).
/// Behavior-oriented, not generic CRUD. Independent of Tour Booking access token.
/// </summary>
public static class PublicHotelBookingCompositionBoundary
{
    public const string PublicApiGroup = "/api/hotel-booking/public";
    public const string AccessTokenHeader = "X-TravelCore-Hotel-Booking-Access-Token";
    public const string IdempotencyHeader = "Idempotency-Key";
    public const string ActorAccountIdClaimType = "tc_account_id";
    public const string PublicHotelBookingIsNotCrud = "Public HotelBooking != CRUD";
    public const string PublicInitiationIsNotConfirmation = "Public HotelBooking initiation != HotelBooking confirmation";
    public const string PendingIsNotConfirmed = "Pending != Confirmed";
    public const string HotelBookingIdIsNotAccessCredential = "HotelBookingId != Access Credential";
    public const string PaymentIdIsNotAccessCredential = "PaymentId != Access Credential";
    public const string SupplierReservationIdIsNotAccessCredential = "SupplierReservationId != Access Credential";
    public const string HotelTokenIsNotTourToken =
        "HotelBooking access token != Tour Booking access token";
    public const string PaymentSucceededIsNotHotelConfirmed =
        "Payment Succeeded != HotelBooking Confirmed";
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const bool PublicListingImplemented = false;
    public const bool GenericCrudImplemented = false;
    public const bool PublicRefundCommandImplemented = false;
    public const bool CardCollectionImplemented = false;
    public const bool OperationalHttpRouteImplemented = false;
    public const bool RawTokenUrlExposureImplemented = false;
    public const bool RawTokenLocalStorageImplemented = false;
}

public static class PublicHotelBookingPresentationStates
{
    public const string NeedsAvailability = "NeedsAvailability";
    public const string AvailabilityPending = "AvailabilityPending";
    public const string HoldActive = "HoldActive";
    public const string NeedsRate = "NeedsRate";
    public const string RateAccepted = "RateAccepted";
    public const string PaymentUnavailable = "PaymentUnavailable";
    public const string ReadyForPayment = "ReadyForPayment";
    public const string PaymentPending = "PaymentPending";
    public const string PaymentReceived = "PaymentReceived";
    public const string SupplierReservationPending = "SupplierReservationPending";
    public const string Confirmed = "Confirmed";
    public const string CancellationAvailable = "CancellationAvailable";
    public const string CancellationPending = "CancellationPending";
    public const string RefundPending = "RefundPending";
    public const string Cancelled = "Cancelled";
    public const string ReconciliationRequired = "ReconciliationRequired";
}
