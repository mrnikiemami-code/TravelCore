using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyDistributionBoundaryTests
{
    [Fact]
    public void AgencyDistributionReference_Describes_Distribution_Intent_Only()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var channel = SalesChannelReference.FromCode("partner-portal");
        var capability = DistributionCapabilityReference.FromCode("distribution-intent");
        var distribution = AgencyDistributionReference.DescribeDistributionIntent(agency, channel, capability);

        Assert.Equal(agency.PartyAgencyId, distribution.Agency.PartyAgencyId);
        Assert.Equal("partner-portal", distribution.Channel.ChannelCode);
        Assert.Equal("distribution-intent", distribution.Capability.CapabilityCode);
    }

    [Fact]
    public void AgencyDistributionBoundary_Preserves_Booking_Pricing_Payment_Ownership()
    {
        Assert.Equal("B2B", AgencyDistributionBoundary.DistributionBoundaryOwner);
        Assert.Equal("Booking", AgencyDistributionBoundary.BookingExecutionOwner);
        Assert.Equal("Pricing", AgencyDistributionBoundary.PricingAuthorityOwner);
        Assert.Equal("Payment", AgencyDistributionBoundary.PaymentExecutionOwner);
        Assert.False(AgencyDistributionBoundary.B2BOwnsBookingExecution);
        Assert.False(AgencyDistributionBoundary.B2BOwnsPricingAuthority);
        Assert.False(AgencyDistributionBoundary.B2BOwnsPaymentExecution);
        Assert.False(AgencyDistributionBoundary.B2BOwnsSalesChannelPersistence);
        Assert.False(AgencyDistributionBoundary.B2BOwnsCommission);
        Assert.False(AgencyDistributionBoundary.SalesChannelTableImplemented);
        Assert.False(AgencyDistributionBoundary.BookingChangesImplemented);
        Assert.False(AgencyDistributionBoundary.PaymentChangesImplemented);
        Assert.False(AgencyDistributionBoundary.PricingChangesImplemented);
    }
}
