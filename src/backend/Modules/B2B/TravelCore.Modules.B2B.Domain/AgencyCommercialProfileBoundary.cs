namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R4: Agency commercial profile boundary — B2B owns future commerce concepts, not money execution or reservation execution.
/// </summary>
public static class AgencyCommercialProfileBoundary
{
    public const string CommercialProfileOwner = "B2B";
    public const string OrganizationIdentityOwner = "Party";
    public const string PricingAuthorityOwner = "Pricing";
    public const string BookingExecutionOwner = "Booking";
    public const string PaymentExecutionOwner = "Payment";
    public const string SettlementExecutionOwner = "Payment";

    public const string CommercialProfileIsNotPayment = "Commercial profile is not Payment";
    public const string CommercialProfileIsNotBooking = "Commercial profile is not Booking";
    public const string CommercialProfileIsNotSettlement = "Commercial profile is not Settlement";

    public const bool B2BOwnsFinancialExecution = false;
    public const bool B2BOwnsPaymentExecution = false;
    public const bool B2BOwnsBookingExecution = false;
    public const bool B2BOwnsPricingAuthority = false;
    public const bool B2BOwnsSettlementExecution = false;
    public const bool AgencyAggregateImplemented = false;
    public const bool ContractImplemented = false;
    public const bool CommissionImplemented = false;
    public const bool CommissionRuleImplemented = false;
    public const bool CreditLimitImplemented = false;
    public const bool WalletImplemented = false;
    public const bool SettlementImplemented = false;
    public const bool InvoiceImplemented = false;
    public const bool CommercialTablesImplemented = false;
    public const bool PaymentChangesImplemented = false;
    public const bool BookingChangesImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
}
