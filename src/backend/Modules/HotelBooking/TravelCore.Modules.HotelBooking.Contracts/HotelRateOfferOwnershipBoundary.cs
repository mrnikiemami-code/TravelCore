namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// P21-R4: HotelBooking owns immutable accepted commercial snapshots. It is not rate authority.
/// </summary>
public static class HotelRateOfferOwnershipBoundary
{
    public const string CommercialRateAuthority = "HotelRateOfferSource";
    public const string NamedHotelSupplier = "NONE";
    public const string ProductionHotelRateSource = "NONE";
    public const string SourcePortName = "IHotelRateOfferSource";
    public const string LiveOfferedRateIsNotMonetarySnapshot =
        "live offered hotel commercial rate != HotelBookingMonetarySnapshot";
    public const string MonetarySnapshotIsNotPayment =
        "HotelBookingMonetarySnapshot != Payment";
    public const string RateOfferIsNotPayment = "HotelRateOfferSnapshot != Payment";
    public const string CancellationTermsAreNotExecution =
        "HotelCancellationPolicySnapshot != cancellation execution";
    public const string CancellationTermsAreNotRefund =
        "HotelCancellationPolicySnapshot != Refund";
    public const string PartialPenaltyIsNotPartialRefund =
        "partial cancellation penalty snapshot != Partial Refund execution";
    public const string RateSourceIsNotAvailabilitySource =
        "Rate source responsibility != Availability source responsibility";
    public const string PricingModuleGeneralized = "NO";
    public const string P20PartialRefund = "DEFERRED";

    public const bool ProductionFakeRateSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool SmartRoutingImplemented = false;
    public const bool HardcodedOfferTtlImplemented = false;
    public const bool ImplicitFxImplemented = false;
    public const bool SilentRepricingImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool HotelBookingStatusImplemented = true;
    public const bool SupplierReservationImplemented = true;
    public const bool PaymentIntegrationImplemented = false;
    public const bool PartialRefundExecutionImplemented = false;
    public const bool CancellationExecutionImplemented = false;
    public const bool PublicRateApiImplemented = false;
}
