namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// P22-R7: Confirmed FlightBooking cancellation process is Flight-owned.
/// Not FlightBookingStatus, not Partial Refund. Public cancellation API remains P22-R8.
/// </summary>
public static class FlightBookingCancellationOwnershipBoundary
{
    public const string ProcessIsNotBookingStatus =
        "FlightBookingCancellation != FlightBookingStatus";
    public const string FareRulesAreNotExecution =
        "FlightFareRulesSnapshot != cancellation execution";
    public const string CancelledIsNotRefundSucceeded =
        "FlightBookingCancelled != RefundSucceeded";
    public const string TicketVoidIsNotPaymentRefund =
        "FlightTicket.Voided != Payment Refund";
    public const string TicketRefundIsNotPaymentRefund =
        "FlightTicket.Refunded != Payment Refund";
    public const string TimeoutIsNotFailed =
        "NetworkTimeout != FlightSupplierReversalAttempt.Failed";
    public const string ClientFlagIsNotCancelled =
        "ClientCancellationSuccess != FlightSupplierReservation.Cancelled";
    public const string CancellationStatuses = "Requested, SupplierReversalPending, RefundPending, Completed";
    public const string AttemptStatuses = "Created, Initiated, Succeeded, Failed";
    public const string AttemptKinds = "TicketVoid, TicketRefund, ReservationCancel";
    public const string FlightBookingStatuses = "Pending, Confirmed, Cancelled";
    public const string TicketStatuses = "Pending, Issued, Voided, Refunded";
    public const string EvaluationTimestampType = "NodaTime.Instant";
    public const string PolicySource = "FlightFareRulesSnapshot.CancelPenalty";
    public const string CancellationTargetBaseline = "Confirmed FlightBooking";
    public const string SourcePortName = "IFlightCancellationSource";
    public const string NamedFlightSupplier = "NONE";
    public const string ProductionFlightCancellationSource = "NONE";
    public const string ProductionPaymentProvider = "NONE";
    public const string PartialRefund = "DEFERRED";
    public const string Amendments = "DEFERRED";
    public const string PerPassengerCancellation = "DEFERRED";
    public const string PartialItineraryCancellation = "DEFERRED";

    public const bool PartialRefundImplemented = false;
    public const bool GenericCancelSurfaceImplemented = false;
    public const bool PendingCustomerCancellationImplemented = false;
    public const bool AmendmentsImplemented = false;
    public const bool RebookingImplemented = false;
    public const bool NoShowExecutionImplemented = false;
    public const bool PerPassengerCancellationImplemented = false;
    public const bool PartialItineraryCancellationImplemented = false;
    public const bool PublicCancellationApiImplemented = false;
    public const bool PublicCancellationUiImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool DistributedTransactionImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool ProductionFakeCancellationSourceImplemented = false;
}
