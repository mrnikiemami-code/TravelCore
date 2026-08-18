namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// P20-R1: Payment is the independent monetary-execution owner (schema <c>payment</c>).
/// Initial logical target is Booking (Tour Booking scope). Payment does not own Booking,
/// Pricing/Quote, BookingMonetarySnapshot, settlement, accounting, or agency payout.
/// No Payment refund / named provider SDK / public UX in T003. Provider-neutral ports + callback route (P20-R3).
/// </summary>
public static class PaymentOwnershipBoundary
{
    public const string OwnerModule = "Payment";
    public const string SchemaName = "payment";
    public const string InitialTarget = "Booking";
    public const string InitialScope = "Tour Booking";
    public const string BookingOwner = "Booking";
    public const string PricingOwner = "Pricing";
    public const string QuoteOwner = "Pricing";
    public const string BookingMonetarySnapshotOwner = "Booking";
    public const string TourOwner = "Tour";
    public const string AgencyMarketplaceOwner = "AgencyMarketplace";
    public const string SearchOwner = "Search";
    public const string SeoOwner = "Seo";
    public const string NotificationOwner = "Notification";
    public const string PresentationOwner = "PublicExperience";
    public const string LogicalReferencePosture = "OpaqueLogicalBookingId";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";
    public const string ProviderSecretPosture = "SecureConfigurationNotSourceControl";

    public const string PaymentIsNotBooking = "Payment != Booking";
    public const string PaymentIsNotPricing = "Payment != Pricing";
    public const string PaymentIsNotQuote = "Payment != Quote";
    public const string PaymentIsNotBookingMonetarySnapshot = "Payment != BookingMonetarySnapshot";
    public const string PaymentIsNotBankSettlement = "Payment != Bank Settlement";
    public const string PaymentIsNotAccountingLedger = "Payment != Accounting Ledger";
    public const string PaymentIsNotAgencySettlement = "Payment != Agency Settlement";
    public const string PaymentStatusIsNotBookingStatus = "PaymentStatus != BookingStatus";
    public const string PaymentSucceededIsNotBookingConfirmed = "PaymentSucceeded != BookingConfirmed";
    public const string BookingCancelledIsNotPaymentRefunded = "BookingCancelled != PaymentRefunded";
    public const string TomanIsNotCurrencyCode = "Toman != CurrencyCode";
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const string ClientAmountIsNotAuthoritative = "ClientAmount != AuthoritativePaymentAmount";
    public const string ClientCurrencyIsNotAuthoritative = "ClientCurrency != AuthoritativePaymentCurrency";
    public const string PaymentIsNotPaymentAttempt = "Payment != PaymentAttempt";
    public const string PaymentStatusIsNotPaymentAttemptStatus = "PaymentStatus != PaymentAttemptStatus";
    public const string FailedAttemptIsNotFailedPayment = "Failed PaymentAttempt != Failed Payment";

    public const bool OwnsBooking = false;
    public const bool OwnsBookingStatus = false;
    public const bool OwnsCapacityHold = false;
    public const bool OwnsBookingMonetarySnapshot = false;
    public const bool OwnsPassengerOrContact = false;
    public const bool OwnsPricing = false;
    public const bool OwnsQuote = false;
    public const bool OwnsTourCatalog = false;
    public const bool OwnsBankSettlement = false;
    public const bool OwnsAccountingLedger = false;
    public const bool OwnsAgencySettlement = false;
    public const bool OwnsNotificationDelivery = false;
    public const bool ProductReferencesAreLogicalOnly = true;
    public const bool ProductReferencesAreSourceOfTruth = false;
    public const bool GeneralizedTargetTypeImplemented = false;
    public const bool PaymentAggregateImplemented = true;
    public const bool PaymentStatusImplemented = true;
    public const bool PaymentAttemptImplemented = true;
    public const bool RefundImplemented = false;
    public const bool ProviderAdapterImplemented = false;
    public const bool ProviderSdkImplemented = false;
    public const bool ProviderPortImplemented = true;
    public const bool CallbackEndpointImplemented = true;
    public const bool PaymentApiImplemented = false;
    public const bool PaymentUiImplemented = false;
    public const bool BookingConfirmImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool CardPanStored = false;
    public const bool CardCvvStored = false;
    public const bool TomanIsCurrencyCode = false;
}
