using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.Modules.Analytics.UnitTests;

public sealed class AnalyticsHardeningBoundaryTests
{
    [Fact]
    public void ConsentInteractionBoundary_Keeps_Ownership_Separated()
    {
        Assert.True(AnalyticsConsentInteractionBoundary.ConsentInteractionBoundaryImplemented);
        Assert.False(AnalyticsConsentInteractionBoundary.ConsentPersistenceImplemented);
        Assert.False(AnalyticsConsentInteractionBoundary.TripPlannerConsentOwnershipTransferred);
        Assert.False(AnalyticsConsentInteractionBoundary.NotificationPreferenceOwnershipTransferred);
    }

    [Fact]
    public void AttributionBoundary_Keeps_Marketing_Vs_Product_Separation()
    {
        Assert.True(AnalyticsAttributionBoundary.AnalyticsOwnsAttributionPosture);
        Assert.False(AnalyticsAttributionBoundary.AttributionPersistenceImplemented);
        Assert.False(AnalyticsAttributionBoundary.OverwritesTripPlannerConsentSnapshots);
        Assert.False(AnalyticsAttributionBoundary.OverwritesNotificationDeliveryPreferences);
    }

    [Fact]
    public void OperationalBoundary_Keeps_Public_Surface_Unimplemented()
    {
        Assert.True(AnalyticsOperationalBoundary.OperationalBoundaryImplemented);
        Assert.False(AnalyticsOperationalBoundary.PublicApiImplemented);
        Assert.False(AnalyticsOperationalBoundary.AdminApiImplemented);
        Assert.False(AnalyticsOperationalBoundary.FakeDispatchSuccessImplemented);
    }

    [Fact]
    public void DeferredScopeBoundary_Keeps_Advanced_Analytics_Deferred()
    {
        Assert.True(AnalyticsDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.WarehouseConnectorImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.BiDashboardImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.MlRecommendationImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.StreamingPipelineImplemented);
        Assert.False(AnalyticsDeferredScopeBoundary.IdentityGraphImplemented);
    }
}
