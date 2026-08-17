using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// Scaffolding smoke — product domain tests arrive with later P13 tasks.
/// </summary>
public sealed class AgencyMarketplaceScaffoldingSmokeTests
{
    [Fact]
    public void AgencyMarketplaceDomainAssembly_IsLoadable()
    {
        var marker = typeof(AgencyMarketplaceDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.AgencyMarketplace.Domain", marker.Namespace);
        Assert.Equal("TravelCore.Modules.AgencyMarketplace.Domain", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void MarketplacePartyId_Is_Logical_Guid_Only()
    {
        var partyId = Guid.Parse("0198b3e0-0000-7000-8000-000000000001");
        var reference = MarketplacePartyId.From(partyId);

        Assert.Equal(partyId, reference.Value);
        Assert.Equal(partyId.ToString("D"), reference.ToString());
        Assert.Throws<ArgumentException>(() => MarketplacePartyId.From(Guid.Empty));
    }

    [Fact]
    public void AgencyPartyIdentityBoundary_Keeps_Party_As_Identity_Source()
    {
        Assert.Equal("Party", AgencyPartyIdentityBoundary.IdentitySourceModule);
        Assert.Equal("AgencyMarketplace", AgencyPartyIdentityBoundary.CommercialLayerModule);
    }
}
