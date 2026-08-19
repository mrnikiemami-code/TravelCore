using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoProgrammaticLandingAndRouteQualityBoundaryTests
{
    [Fact]
    public void ProgrammaticLandingBoundary_Forbids_Thin_Factory()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.ProgrammaticLandingBoundaryImplemented);
        Assert.True(SeoProgrammaticLandingBoundary.ControlledLandingPostureImplemented);
        Assert.False(SeoProgrammaticLandingBoundary.BulkThinUrlFactoryImplemented);
        Assert.False(SeoProgrammaticLandingBoundary.AiLandingCopyImplemented);
    }

    [Fact]
    public void RouteQualityBoundary_Keeps_Observability_Only()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.RouteQualityBoundaryImplemented);
        Assert.True(SeoRouteQualityBoundary.OrphanDetectionImplemented);
        Assert.False(SeoRouteQualityBoundary.FakeIndexSuccessImplemented);
    }
}
