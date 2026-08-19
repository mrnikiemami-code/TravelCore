namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// P22-R5: Flight owns supplier reservation / PNR correlation. Not a type named PNR,
/// not ticketing, Payment, or customer cancellation.
/// </summary>
public static class FlightReservationOwnershipBoundary
{
    public const string ReservationAuthority = "FlightReservationSource";
    public const string NamedFlightSupplier = "NONE";
    public const string ProductionFlightReservationSource = "NONE";
    public const string SourcePortName = "IFlightReservationSource";
    public const string ReservationIsNotFlightBooking =
        "FlightSupplierReservation != FlightBooking";
    public const string ReservationIsNotAttempt =
        "FlightSupplierReservation != FlightSupplierReservationAttempt";
    public const string LocatorIsNotPnrType = "ReservationLocator != type PNR";
    public const string LocatorIsNotInternalId =
        "ReservationLocator != FlightBookingId != FlightSupplierReservationId != SourceReservationReference";
    public const string TimeoutIsNotFailed =
        "NetworkTimeout != FlightSupplierReservationAttempt.Failed";
    public const string ConfirmationIsNotPayment =
        "FlightSupplierReservation.Confirmed != Payment succeeded";
    public const string ConfirmationIsNotTicket =
        "FlightSupplierReservation.Confirmed != ticket issued";
    public const string FlightBookingConfirmedRequiresTripleEvidence =
        "FlightBooking.Confirmed = Reservation.Confirmed AND Payment.Succeeded AND all required tickets Issued";
    public const string Capabilities = "ReservationCreate, ReservationQuery";
    public const string TicketingCapabilities = "TicketCreate, TicketQuery";
    public const string OfferExpiryIsNotReservationExpiry =
        "OfferExpiresAt != ReservationExpiresAt";
    public const string TicketingDeadlineIsNotReservationExpiry =
        "TicketingDeadline != ReservationExpiresAt";
    public const string ReservationStatuses = "Pending, Confirmed, Expired, Cancelled";
    public const string AttemptStatuses = "Created, Initiated, Confirmed, Failed";

    public const bool ProductionFakeReservationSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool SmartRoutingImplemented = false;
    public const bool HardcodedReservationTtlImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool PaymentRequiredForReservation = false;
    public const bool FlightBookingStatusImplemented = true;
    public const bool PnrTypeImplemented = false;
    public const bool TicketImplemented = true;
    public const bool CancellationExecutionImplemented = false;
    public const bool PublicReservationApiImplemented = false;
}
