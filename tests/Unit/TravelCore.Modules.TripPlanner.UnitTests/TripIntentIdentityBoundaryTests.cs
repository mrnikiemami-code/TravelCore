using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Anonymous-first planner identity boundary (TC-P18-T003 / P18-R3).
/// </summary>
public sealed class TripIntentIdentityBoundaryTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);

    [Fact]
    public void Create_Does_Not_Require_Actor_Or_Account()
    {
        var intent = TripIntent.Create(Now, "Anonymous planning");

        Assert.Null(intent.ActorReference);
        Assert.NotNull(intent.DraftAccessToken.Value);
        Assert.True(intent.DraftAccessToken.Value.Length >= 32);
        Assert.Null(typeof(TripIntent).GetProperty("AccountId"));
        Assert.Null(typeof(TripIntent).GetProperty("PartyId"));
        Assert.Null(typeof(TripIntent).GetProperty("UserId"));
    }

    [Fact]
    public void AssociateActor_Is_Optional_Opaque_Reference()
    {
        var intent = TripIntent.Create(Now);
        var actorId = Guid.Parse("0198b3e0-0000-7000-8000-000000000051");
        var later = Instant.FromUtc(2026, 8, 18, 13, 0);
        intent.AssociateActor(new PlannerActorReference(actorId), later);

        Assert.Equal(actorId, intent.ActorReference!.Value.ActorId);
        Assert.Equal(2, intent.PlanningRevision);
        Assert.Equal(later, intent.UpdatedAt);
        Assert.False(typeof(PlannerActorReference).IsClass);
    }

    [Fact]
    public void DraftAccessToken_Is_Unique_Per_Intent()
    {
        var first = TripIntent.Create(Now);
        var second = TripIntent.Create(Now);

        Assert.NotEqual(first.DraftAccessToken.Value, second.DraftAccessToken.Value);
    }
}
