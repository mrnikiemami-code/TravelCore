using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyCommercialProfileBoundaryTests
{
    [Fact]
    public void AgencyBusinessReference_Describes_Commercial_Intent_Only()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var capability = CommercialCapabilityReference.FromCode("partner-booking-intent");
        var business = AgencyBusinessReference.DescribeCommercialIntent(agency, capability);

        Assert.Equal(agency.PartyAgencyId, business.Agency.PartyAgencyId);
        Assert.Equal("partner-booking-intent", business.Capability.CapabilityCode);
    }

    [Fact]
    public void AgencyCommercialProfileBoundary_Preserves_Execution_Ownership()
    {
        Assert.Equal("B2B", AgencyCommercialProfileBoundary.CommercialProfileOwner);
        Assert.Equal("Party", AgencyCommercialProfileBoundary.OrganizationIdentityOwner);
        Assert.Equal("Pricing", AgencyCommercialProfileBoundary.PricingAuthorityOwner);
        Assert.Equal("Booking", AgencyCommercialProfileBoundary.BookingExecutionOwner);
        Assert.Equal("Payment", AgencyCommercialProfileBoundary.PaymentExecutionOwner);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsFinancialExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsPaymentExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsBookingExecution);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsPricingAuthority);
        Assert.False(AgencyCommercialProfileBoundary.B2BOwnsSettlementExecution);
        Assert.False(AgencyCommercialProfileBoundary.ContractImplemented);
        Assert.False(AgencyCommercialProfileBoundary.CommissionImplemented);
        Assert.False(AgencyCommercialProfileBoundary.SettlementImplemented);
        Assert.False(AgencyCommercialProfileBoundary.CommercialTablesImplemented);
    }

    [Fact]
    public void Domain_Does_Not_Define_Forbidden_Commercial_Product()
    {
        var domain = typeof(B2BDomainAssemblyMarker).Assembly;
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Agency"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Contract"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Commission"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.CommissionRule"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.CreditLimit"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Wallet"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Settlement"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Invoice"));
    }
}
