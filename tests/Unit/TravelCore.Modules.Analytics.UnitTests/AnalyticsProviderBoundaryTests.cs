using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.Modules.Analytics.UnitTests;

public sealed class AnalyticsProviderBoundaryTests
{
    [Fact]
    public void AnalyticsProviderKey_Normalizes_Controlled_Identifier()
    {
        Assert.True(AnalyticsProviderKey.TryParse("internal-zero", out var key));
        Assert.Equal("internal-zero", key.Value);
    }

    [Fact]
    public void AnalyticsDispatchRequest_Keeps_Provider_Neutral_Shape()
    {
        var key = new AnalyticsProviderKey("internal-zero");
        var request = new AnalyticsDispatchRequest(
            key,
            AnalyticsProductEventKind.SearchPerformed,
            SourceModule: "Search",
            CorrelationReference: "corr-1",
            ResourceReference: "search:query-hash",
            OccurredAtReference: "2026-08-19T19:00:00Z");

        Assert.Equal(AnalyticsProductEventKind.SearchPerformed, request.EventKind);
        Assert.Equal("internal-zero", request.ProviderKey.Value);
    }

    [Fact]
    public void OwnershipBoundary_Records_T006_Provider_Port_Posture()
    {
        Assert.True(AnalyticsOwnershipBoundary.ProviderPortImplemented);
        Assert.True(AnalyticsOwnershipBoundary.ProviderAbstractionImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProviderImplemented);
        Assert.True(AnalyticsProviderTrustBoundary.ZeroProviderPostureValid);
        Assert.False(AnalyticsProviderBoundary.NamedProductionAdapterImplemented);
    }
}
