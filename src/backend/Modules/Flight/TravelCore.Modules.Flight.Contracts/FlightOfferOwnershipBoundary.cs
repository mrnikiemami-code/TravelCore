namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// P22-R4: Flight owns immutable accepted commercial snapshots. The source is fare authority.
/// </summary>
public static class FlightOfferOwnershipBoundary
{
    public const string CommercialFareAuthority = "FlightOfferSource";
    public const string NamedFlightSupplier = "NONE";
    public const string ProductionFlightOfferSource = "NONE";
    public const string SourcePortName = "IFlightOfferSource";
    public const string LiveSearchPriceIsNotMonetarySnapshot =
        "search-result price != FlightBookingMonetarySnapshot";
    public const string AvailabilityIsNotFareAuthority =
        "R3 Available != R4 commercial revalidation";
    public const string MonetarySnapshotIsNotPayment =
        "FlightBookingMonetarySnapshot != Payment";
    public const string OfferSnapshotIsNotPayment = "FlightOfferSnapshot != Payment";
    public const string OfferSnapshotIsNotSearchResult =
        "FlightOfferSnapshot != FlightSearchResult";
    public const string FareRulesAreNotCancellationExecution =
        "FlightFareRulesSnapshot != cancellation execution";
    public const string FareRulesAreNotRefund = "FlightFareRulesSnapshot != Refund";
    public const string PartialRefundFactIsNotExecution =
        "partial-refund-required fact != Partial Refund execution";
    public const string TicketingDeadlineIsNotOfferExpiry =
        "TicketingDeadline != OfferExpiresAt";
    public const string PricingModuleGeneralized = "NO";
    public const string P20PartialRefund = "DEFERRED";
    public const string MoneyModel = "TravelCore.Money";
    public const string TomanIsNotCurrencyCode = "Toman != CurrencyCode";

    public const bool ProductionFakeOfferSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool SmartRoutingImplemented = false;
    public const bool HardcodedOfferTtlImplemented = false;
    public const bool ImplicitFxImplemented = false;
    public const bool SilentRepricingImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool FlightBookingStatusImplemented = false;
    public const bool PnrImplemented = false;
    public const bool TicketImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
    public const bool PartialRefundExecutionImplemented = false;
    public const bool CancellationExecutionImplemented = false;
    public const bool AncillariesImplemented = false;
    public const bool PublicOfferApiImplemented = false;
}
