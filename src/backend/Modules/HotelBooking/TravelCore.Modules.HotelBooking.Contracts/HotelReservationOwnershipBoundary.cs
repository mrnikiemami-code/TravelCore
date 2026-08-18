namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// P21-R5: HotelBooking owns lifecycle and final supplier-reservation correlation.
/// It is not a named supplier, Payment, or cancellation execution.
/// </summary>
public static class HotelReservationOwnershipBoundary
{
    public const string ReservationAuthority = "HotelReservationSource";
    public const string NamedHotelSupplier = "NONE";
    public const string ProductionHotelReservationSource = "NONE";
    public const string SourcePortName = "IHotelReservationSource";
    public const string HoldIsNotReservation = "HotelAvailabilityHold != HotelSupplierReservation";
    public const string ReservationIsNotHotelBooking = "HotelSupplierReservation != HotelBooking";
    public const string BookingStatusIsNotSupplierStatus =
        "HotelBookingStatus != HotelSupplierReservationStatus";
    public const string ReservationIsNotAttempt =
        "HotelSupplierReservation != HotelSupplierReservationAttempt";
    public const string TimeoutIsNotFailed =
        "NetworkTimeout != HotelSupplierReservationAttempt.Failed";
    public const string ClientFlagIsNotConfirmation =
        "ClientReservationSuccess != HotelSupplierReservation.Confirmed";
    public const string BrowserReturnIsNotConfirmation =
        "BrowserReturn != HotelBooking.Confirmed";
    public const string UnverifiedCallbackIsNotConfirmation =
        "UnverifiedSupplierCallback != HotelSupplierReservation.Confirmed";
    public const string ConfirmationIsNotPayment = "HotelBooking.Confirmed != Payment succeeded";
    public const string HotelBookingStatuses = "Pending, Confirmed, Cancelled";
    public const string ReservationStatuses = "Pending, Confirmed, Cancelled";
    public const string AttemptStatuses = "Created, Initiated, Confirmed, Failed";

    public const bool ProductionFakeReservationSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool SmartRoutingImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool PaymentRequiredForConfirmation = true;
    public const bool CancellationExecutionImplemented = true;
    public const bool PublicReservationApiImplemented = false;
}
