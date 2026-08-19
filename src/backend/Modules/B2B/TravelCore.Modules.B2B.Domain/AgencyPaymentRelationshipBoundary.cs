namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R6: Agency commerce payment boundary.
/// B2B describes relationship intent only; Payment remains execution owner.
/// </summary>
public static class AgencyPaymentRelationshipBoundary
{
    public const string CommerceBoundaryOwner = "B2B";
    public const string PaymentExecutionOwner = "Payment";
    public const string BookingExecutionOwner = "Booking";
    public const string MoneyMovementOwner = "Payment";

    public const string RelationshipIsNotPaymentExecution = "Relationship is not Payment execution";
    public const string RelationshipIsNotMoneyMovement = "Relationship is not money movement";

    public const bool B2BOwnsPaymentExecution = false;
    public const bool B2BModifiesPaymentTargets = false;
    public const bool B2BOwnsMoneyMovement = false;
    public const bool WalletImplemented = false;
    public const bool CreditImplemented = false;
    public const bool SettlementImplemented = false;
    public const bool InvoiceImplemented = false;
    public const bool CommissionPayoutImplemented = false;
    public const bool AgencyBalanceImplemented = false;
    public const bool PaymentApiChangesImplemented = false;
    public const bool BookingChangesImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
}
