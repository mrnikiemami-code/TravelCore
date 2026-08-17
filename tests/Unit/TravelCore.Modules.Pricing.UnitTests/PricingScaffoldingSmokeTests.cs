using TravelCore.Modules.Pricing.Domain;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Scaffolding smoke — product domain tests arrive with later P12 tasks.
/// </summary>
public sealed class PricingScaffoldingSmokeTests
{
    [Fact]
    public void PricingDomainAssembly_IsLoadable()
    {
        var marker = typeof(PricingDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Pricing.Domain", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Pricing.Domain", marker.Assembly.GetName().Name);
    }
}
