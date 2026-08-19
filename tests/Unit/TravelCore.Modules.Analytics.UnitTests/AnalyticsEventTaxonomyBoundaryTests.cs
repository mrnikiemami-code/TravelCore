using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.Modules.Analytics.UnitTests;

public sealed class AnalyticsEventTaxonomyBoundaryTests
{
    [Fact]
    public void AnalyticsEventReference_Exposes_All_Roadmap_Event_Kinds()
    {
        Assert.Equal(AnalyticsProductEventKind.SearchPerformed, AnalyticsEventReference.SearchPerformed.Kind);
        Assert.Equal(AnalyticsProductEventKind.SearchResultClicked, AnalyticsEventReference.SearchResultClicked.Kind);
        Assert.Equal(AnalyticsProductEventKind.SearchNoResults, AnalyticsEventReference.SearchNoResults.Kind);
        Assert.Equal(AnalyticsProductEventKind.FilterApplied, AnalyticsEventReference.FilterApplied.Kind);
        Assert.Equal(AnalyticsProductEventKind.TourViewed, AnalyticsEventReference.TourViewed.Kind);
        Assert.Equal(AnalyticsProductEventKind.HotelViewed, AnalyticsEventReference.HotelViewed.Kind);
        Assert.Equal(AnalyticsProductEventKind.QuoteCreated, AnalyticsEventReference.QuoteCreated.Kind);
        Assert.Equal(AnalyticsProductEventKind.BookingStarted, AnalyticsEventReference.BookingStarted.Kind);
        Assert.Equal(AnalyticsProductEventKind.BookingCompleted, AnalyticsEventReference.BookingCompleted.Kind);
    }

    [Fact]
    public void AnalyticsSemanticEventEnvelope_Keeps_Opaque_References()
    {
        var envelope = new AnalyticsSemanticEventEnvelope(
            AnalyticsProductEventKind.TourViewed,
            SourceModule: "Tour",
            CorrelationReference: "corr-1",
            ResourceReference: "tour:0198a000-0000-7000-8000-0000000000aa",
            OccurredAtReference: "2026-08-19T18:00:00Z");

        Assert.Equal(AnalyticsProductEventKind.TourViewed, envelope.EventKind);
        Assert.Equal("Tour", envelope.SourceModule);
        Assert.DoesNotContain("@", envelope.ResourceReference);
    }

    [Fact]
    public void OwnershipBoundary_Records_T005_EventTaxonomy_Posture()
    {
        Assert.True(AnalyticsOwnershipBoundary.EventTaxonomyBoundaryImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.EventPersistenceImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.ProviderDispatchImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.PublicApiImplemented);
    }
}
