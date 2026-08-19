namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R5: Agency distribution is a commerce boundary — not Booking, Pricing, Payment, or sales execution.
/// </summary>
public static class AgencyDistributionBoundary
{
    public const string DistributionBoundaryOwner = "B2B";
    public const string BookingExecutionOwner = "Booking";
    public const string PricingAuthorityOwner = "Pricing";
    public const string PaymentExecutionOwner = "Payment";
    public const string OrganizationIdentityOwner = "Party";

    public const string DistributionIsNotSalesImplementation = "Distribution is not Sales implementation";
    public const string DistributionIsNotBooking = "Distribution is not Booking";
    public const string DistributionIsNotPricing = "Distribution is not Pricing";
    public const string DistributionIsNotPayment = "Distribution is not Payment";

    public const bool B2BOwnsBookingExecution = false;
    public const bool B2BOwnsPricingAuthority = false;
    public const bool B2BOwnsPaymentExecution = false;
    public const bool B2BOwnsSalesChannelPersistence = false;
    public const bool B2BOwnsCommission = false;
    public const bool B2BOwnsAgencyPricing = false;
    public const bool B2BOwnsDiscountRules = false;
    public const bool B2BOwnsContract = false;
    public const bool B2BOwnsSettlement = false;
    public const bool B2BOwnsWallet = false;
    public const bool SalesChannelTableImplemented = false;
    public const bool DistributionProductTablesImplemented = false;
    public const bool BookingChangesImplemented = false;
    public const bool PaymentChangesImplemented = false;
    public const bool PricingChangesImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
}
