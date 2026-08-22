namespace TravelCore.Modules.CommercialFinance.Contracts;

/// <summary>
/// P39-R1: Commercial Finance owns commission agreements, obligations, settlement, and payout instructions.
/// Evidence from AgencyMarketplace/Booking/Payment is read-only; no cross-schema FK or execution ownership.
/// </summary>
public static class CommercialFinanceOwnershipBoundary
{
    public const string OwnerModule = "CommercialFinance";
    public const string SchemaName = "commercial_finance";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";

    public const string CommissionIsNotPricing = "Commission != Pricing";
    public const string SettlementIsNotPayment = "Settlement != Payment";
    public const string PayoutIsNotBooking = "Payout != Booking";
    public const string ObligationIsNotInvoice = "Commercial Obligation != Invoice";
    public const string AuditIsNotFinancialLedger = "Audit != Financial Ledger";
    public const string AgencyOfferIsNotFinancialTransaction = "AgencyOffer != Financial Transaction";

    public const string AgencyMarketplaceOwner = "AgencyMarketplace";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string PricingOwner = "Pricing";

    public const bool OwnsAgencyOfferLifecycle = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool OwnsTravelerPricing = false;
    public const bool MutatesAgencyOffer = false;
    public const bool CommissionFormulaImplemented = false;
    public const bool SettlementJobImplemented = false;
    public const bool PayoutExecutionImplemented = false;
    public const bool TaxExecutionImplemented = false;
    public const bool FxExecutionImplemented = false;
    public const bool PaymentEventHandlerImplemented = false;
    public const bool SeparateCommercialFinanceModuleImplemented = true;
    public const bool SeparateCommercialFinanceSchemaImplemented = true;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool SharedDbContextImplemented = false;
}
