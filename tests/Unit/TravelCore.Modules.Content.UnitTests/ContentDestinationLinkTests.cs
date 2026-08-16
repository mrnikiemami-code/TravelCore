using NodaTime;
using TravelCore.Modules.Content.Domain;
using Xunit;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class ContentDestinationLinkTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 23, 30);

    [Fact]
    public void AssignDestination_IsIdempotentAndBounded()
    {
        var item = ContentItemAggregate.CreateArticle("ART-DST-1", "Dest Demo", Now);
        var destinationId = Guid.NewGuid();

        item.AssignDestination(destinationId, Now);
        item.AssignDestination(destinationId, Now);

        Assert.Single(item.Destinations);
        Assert.Equal(destinationId, item.Destinations.First().DestinationId);
        Assert.True(item.RemoveDestination(destinationId, Now));
        Assert.Empty(item.Destinations);
    }

    [Fact]
    public void AssignDestination_RejectsEmpty()
    {
        var item = ContentItemAggregate.CreateLandingPage("LND-DST-1", "Landing", Now);
        Assert.ThrowsAny<ArgumentException>(() => item.AssignDestination(Guid.Empty, Now));
    }
}
