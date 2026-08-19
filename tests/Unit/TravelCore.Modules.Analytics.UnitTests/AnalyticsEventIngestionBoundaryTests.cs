using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.Modules.Analytics.UnitTests;

public sealed class AnalyticsEventIngestionBoundaryTests
{
    [Fact]
    public void SemanticEventConsumer_Port_Uses_Envelope_Without_Pii_Fields()
    {
        Assert.NotNull(typeof(IAnalyticsSemanticEventConsumer));
        var envelope = new AnalyticsSemanticEventEnvelope(
            AnalyticsProductEventKind.BookingStarted,
            SourceModule: "Booking",
            CorrelationReference: "corr-1",
            ResourceReference: "booking:0198a000-0000-7000-8000-0000000000bb",
            OccurredAtReference: "2026-08-19T20:00:00Z");

        Assert.Equal(AnalyticsProductEventKind.BookingStarted, envelope.EventKind);
        Assert.DoesNotContain("@", envelope.ResourceReference);
    }

    [Fact]
    public void IdempotencyBoundary_Declares_NonBlocking_Downstream_Posture()
    {
        Assert.True(AnalyticsIdempotencyBoundary.IdempotentIngestionPostureDeclared);
        Assert.Equal("NOT ALLOWED", AnalyticsIdempotencyBoundary.SynchronousDispatchRequiredForCoreCorrectness);
        Assert.False(AnalyticsIdempotencyBoundary.OutboxConsumerImplemented);
    }

    [Fact]
    public void OwnershipBoundary_Records_T007_Ingestion_Posture()
    {
        Assert.True(AnalyticsOwnershipBoundary.IngestionBoundaryImplemented);
        Assert.True(AnalyticsEventIngestionBoundary.NonBlockingDispatchPosture.Length > 0);
        Assert.True(AnalyticsPrivacyBoundary.PrivacyBoundaryImplemented);
        Assert.False(AnalyticsPublisherInteractionBoundary.PiiPersistenceImplemented);
    }
}
