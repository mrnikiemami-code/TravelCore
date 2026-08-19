namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R7: Operational boundary for agency commerce.
/// B2B describes future operational concepts only and does not own authorization or execution.
/// </summary>
public static class AgencyOperationalBoundary
{
    public const string OperationalBoundaryOwner = "B2B";
    public const string AuthorizationOwner = "Access";
    public const string IdentityOwner = "Identity";
    public const string BookingExecutionOwner = "Booking";
    public const string PaymentExecutionOwner = "Payment";

    public const bool B2BOwnsAuthorization = false;
    public const bool B2BExposesOperationalMutation = false;
    public const bool B2BModifiesBookingOperations = false;
    public const bool B2BModifiesPaymentOperations = false;
    public const bool AdminApiImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool DashboardImplemented = false;
    public const bool ReportingEngineImplemented = false;
    public const bool AuditSystemImplemented = false;
    public const bool UserManagementImplemented = false;
    public const bool PermissionChangesImplemented = false;
    public const bool OperationalTablesImplemented = false;
    public const bool FrontendImplemented = false;
}
