namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R6 Booking/Payment/confirmation/cancellation orchestration boundary.
/// Payment execution is outside P19. Executable confirmation is DEFERRED.
/// </summary>
public static class BookingOrchestrationBoundary
{
    public const string BookingIsNotPayment = "Booking != Payment";
    public const string BookingStatusIsNotPaymentStatus = "BookingStatus != PaymentStatus";
    public const string BookingMonetarySnapshotIsNotPaymentTransaction = "BookingMonetarySnapshot != PaymentTransaction";
    public const string PaymentSucceededIsNotBookingConfirmed = "PaymentSucceeded != BookingConfirmed";
    public const string BookingCancelledIsNotPaymentRefunded = "BookingCancelled != PaymentRefunded";
    public const string ExecutableConfirmWorkflow = "DEFERRED to Payment integration";
    public const string ConfirmedCancellation = "DEFERRED";
    public const bool FakePaymentImplemented = false;
    public const bool PaymentDrivenConfirmationImplemented = false;
    public const bool CallerControlledPaymentBooleanImplemented = false;
    public const bool ConfirmedToCancelledImplemented = false;
    public const bool PendingCancellationImplemented = true;
    public const bool PendingCancellationReleasesActiveHold = true;
}
