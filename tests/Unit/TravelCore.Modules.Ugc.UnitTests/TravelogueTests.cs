using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Travelogue UGC narrative (TC-P16-T004 / P16-R4). Travelogue != ContentItem.
/// </summary>
public sealed class TravelogueTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid Actor = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");

    [Fact]
    public void Create_Owns_Narrative_With_Opaque_Actor_And_Locale()
    {
        var travelogue = Travelogue.Create(Actor, "en-us", "  Cappadocia  ", "Balloons at dawn.", Now);

        Assert.NotEqual(Guid.Empty, travelogue.Id.Value);
        Assert.Equal(Actor, travelogue.ActorId);
        Assert.Equal("en-US", travelogue.LocaleCode);
        Assert.Equal("Cappadocia", travelogue.Title);
        Assert.Equal("Balloons at dawn.", travelogue.Body);
        Assert.Equal(Now, travelogue.CreatedAt);
        Assert.True(UgcOwnershipBoundary.TravelogueImplemented);
        Assert.True(UgcOwnershipBoundary.TravelogueIsNotContentItem);
        Assert.False(UgcOwnershipBoundary.OwnsContentCms);
        Assert.True(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.Equal(PublicationStatus.Draft, travelogue.PublicationStatus);
        Assert.Equal(ModerationStatus.Pending, travelogue.ModerationStatus);
        Assert.Null(typeof(Travelogue).GetProperty("ContentItemId"));
        Assert.Null(typeof(Travelogue).GetProperty("TargetId"));
    }

    [Fact]
    public void Create_Rejects_Empty_Actor_Locale_Or_Text()
    {
        Assert.Throws<ArgumentException>(() => Travelogue.Create(Guid.Empty, "fa", "T", "B", Now));
        Assert.Throws<ArgumentException>(() => Travelogue.Create(Actor, "  ", "T", "B", Now));
        Assert.Throws<ArgumentException>(() => Travelogue.Create(Actor, "fa", "  ", "B", Now));
        Assert.Throws<ArgumentException>(() => Travelogue.Create(Actor, "fa", "T", "  ", Now));
        Assert.Throws<ArgumentException>(() =>
            Travelogue.Create(Actor, "fa", new string('a', Travelogue.TitleMaxLength + 1), "B", Now));
    }

    [Fact]
    public void SetText_And_SetLocale_Update_Narrative()
    {
        var travelogue = Travelogue.Create(Actor, "fa", "A", "B", Now);
        var later = Instant.FromUtc(2026, 8, 17, 22, 0);
        travelogue.SetText("New title", "New body", later);
        travelogue.SetLocale("en", later);
        Assert.Equal("New title", travelogue.Title);
        Assert.Equal("New body", travelogue.Body);
        Assert.Equal("en", travelogue.LocaleCode);
        Assert.Equal(later, travelogue.UpdatedAt);
    }
}
