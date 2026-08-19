using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P26-T008 / P26-R6-R8: hardening guardrails for graph operational/deferred posture.
/// </summary>
public sealed class SeoGraphHardeningGuardrailTests
{
    [Fact]
    public void SeoGraphHardening_Guardrails_Are_Declared()
    {
        Assert.True(SeoContentGraphOwnershipBoundary.HardeningGuardrailsImplemented);
        Assert.True(SeoContentGraphOwnershipBoundary.SitemapGraphAwarenessImplemented);
        Assert.True(SeoContentGraphOwnershipBoundary.OperationalBoundaryImplemented);
        Assert.True(SeoContentGraphOwnershipBoundary.DeferredScopeBoundaryImplemented);
        Assert.True(SeoSitemapGraphCompletenessBoundary.GraphAwareSitemapPostureImplemented);
        Assert.True(SeoGraphOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(SeoGraphDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
        Assert.Equal("DEFERRED", SeoGraphDeferredScopeBoundary.ExternalLinkCrawling);
        Assert.Equal("No fake index success", SeoGraphOperationalBoundary.NoFakeIndexSuccess);
    }
}
