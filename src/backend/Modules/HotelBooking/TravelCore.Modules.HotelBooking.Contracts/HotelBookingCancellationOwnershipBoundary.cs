namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// P21-R7: Confirmed HotelBooking cancellation process is HotelBooking-owned.
/// Not HotelBookingStatus, not Partial Refund, not public API.
/// </summary>
public static class HotelBookingCancellationOwnershipBoundary
{
    public const string ProcessIsNotBookingStatus =
        "HotelBookingCancellation != HotelBookingStatus";
    public const string PolicyIsNotExecution =
        "HotelCancellationPolicySnapshot != cancellation execution";
    public const string CancelledIsNotRefundSucceeded =
        "HotelBookingCancelled != RefundSucceeded";
    public const string TimeoutIsNotFailed =
        "NetworkTimeout != HotelSupplierCancellationAttempt.Failed";
    public const string ClientFlagIsNotCancelled =
        "ClientCancellationSuccess != HotelSupplierReservation.Cancelled";
    public const string CancellationStatuses = "Requested, SupplierCancellationPending, RefundPending, Completed";
    public const string AttemptStatuses = "Created, Initiated, Confirmed, Failed";
    public const string HotelBookingStatuses = "Pending, Confirmed, Cancelled";
    public const string EvaluationTimestampType = "NodaTime.Instant";
    public const string PolicySource = "HotelCancellationPolicySnapshot";
    public const string CancellationTargetBaseline = "Confirmed HotelBooking";
    public const string NamedHotelSupplier = "NONE";
    public const string ProductionHotelReservationSource = "NONE";
    public const string ProductionPaymentProvider = "NONE";
    public const string PartialRefund = "DEFERRED";
    public const string Amendments = "DEFERRED";
    public const string PayAtProperty = "DEFERRED";
    public const string Deposit = "DEFERRED";

    public const bool PartialRefundImplemented = false;
    public const bool GenericCancelSurfaceImplemented = false;
    public const bool PendingCustomerCancellationImplemented = false;
    public const bool AmendmentsImplemented = false;
    public const bool RebookingImplemented = false;
    public const bool NoShowExecutionImplemented = false;
    public const bool PublicCancellationApiImplemented = false;
    public const bool PublicCancellationUiImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool DistributedTransactionImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool ProductionFakeReservationSourceImplemented = false;
}
