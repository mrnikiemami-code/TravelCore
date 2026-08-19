using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyOperationalBoundaryTests
{
    [Fact]
    public void AgencyOperationalReference_Describes_Operational_Intent_Only()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var reporting = AgencyReportingReference.FromCode("operational-read-intent");
        var capability = AgencyOperationalCapabilityReference.FromCode("agency-operational-intent");
        var operational = AgencyOperationalReference.DescribeOperationalIntent(agency, reporting, capability);

        Assert.Equal(agency.PartyAgencyId, operational.Agency.PartyAgencyId);
        Assert.Equal("operational-read-intent", operational.Reporting.ReportingCode);
        Assert.Equal("agency-operational-intent", operational.Capability.CapabilityCode);
    }

    [Fact]
    public void AgencyOperationalBoundary_Preserves_Authorization_And_Execution_Ownership()
    {
        Assert.Equal("B2B", AgencyOperationalBoundary.OperationalBoundaryOwner);
        Assert.Equal("Access", AgencyOperationalBoundary.AuthorizationOwner);
        Assert.Equal("Booking", AgencyOperationalBoundary.BookingExecutionOwner);
        Assert.Equal("Payment", AgencyOperationalBoundary.PaymentExecutionOwner);
        Assert.False(AgencyOperationalBoundary.B2BOwnsAuthorization);
        Assert.False(AgencyOperationalBoundary.B2BExposesOperationalMutation);
        Assert.False(AgencyOperationalBoundary.B2BModifiesBookingOperations);
        Assert.False(AgencyOperationalBoundary.B2BModifiesPaymentOperations);
        Assert.False(AgencyOperationalBoundary.AdminApiImplemented);
        Assert.False(AgencyOperationalBoundary.PublicApiImplemented);
        Assert.False(AgencyOperationalBoundary.DashboardImplemented);
    }
}
