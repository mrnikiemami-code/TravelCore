using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyPaymentBoundaryTests
{
    [Fact]
    public void AgencyPaymentReference_Describes_Relationship_Intent_Only()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var responsibility = PaymentResponsibilityReference.FromCode("agency-customer-payment-intent");
        var capability = CommercialPaymentCapabilityReference.FromCode("payment-relationship-intent");
        var relation = AgencyPaymentReference.DescribeRelationship(agency, responsibility, capability);

        Assert.Equal(agency.PartyAgencyId, relation.Agency.PartyAgencyId);
        Assert.Equal("agency-customer-payment-intent", relation.Responsibility.ResponsibilityCode);
        Assert.Equal("payment-relationship-intent", relation.Capability.CapabilityCode);
    }

    [Fact]
    public void AgencyPaymentBoundary_Preserves_Payment_Ownership()
    {
        Assert.Equal("B2B", AgencyPaymentRelationshipBoundary.CommerceBoundaryOwner);
        Assert.Equal("Payment", AgencyPaymentRelationshipBoundary.PaymentExecutionOwner);
        Assert.False(AgencyPaymentRelationshipBoundary.B2BOwnsPaymentExecution);
        Assert.False(AgencyPaymentRelationshipBoundary.B2BModifiesPaymentTargets);
        Assert.False(AgencyPaymentRelationshipBoundary.B2BOwnsMoneyMovement);
        Assert.False(AgencyPaymentRelationshipBoundary.WalletImplemented);
        Assert.False(AgencyPaymentRelationshipBoundary.CreditImplemented);
        Assert.False(AgencyPaymentRelationshipBoundary.SettlementImplemented);
    }
}
