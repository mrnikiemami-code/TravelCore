using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Flat UGC Comment (TC-P16-T006 / P16-R6). Like deferred. No threading / moderation.
/// </summary>
public sealed class CommentTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid Actor = Guid.Parse("0198b3e0-0000-7000-8000-000000000081");
    private static readonly Guid ReviewId = Guid.Parse("0198b3e0-0000-7000-8000-000000000082");

    [Fact]
    public void Create_Owns_Flat_Comment_On_Review()
    {
        var comment = Comment.Create(Actor, "Review", ReviewId, "  Great tour.  ", Now);

        Assert.NotEqual(Guid.Empty, comment.Id.Value);
        Assert.Equal(Actor, comment.ActorId);
        Assert.Equal(CommentTargetType.Review, comment.TargetType);
        Assert.Equal(ReviewId, comment.TargetId);
        Assert.Equal("Great tour.", comment.Body);
        Assert.True(UgcOwnershipBoundary.CommentImplemented);
        Assert.True(UgcOwnershipBoundary.LikeDeferred);
        Assert.False(UgcOwnershipBoundary.LikeImplemented);
        Assert.True(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.Equal(ModerationStatus.Pending, comment.ModerationStatus);
        Assert.Equal(PublicationStatus.Hidden, comment.PublicationStatus);
        Assert.Null(typeof(Comment).GetProperty("ParentCommentId"));
        Assert.Null(typeof(Comment).GetProperty("LikeCount"));
    }

    [Fact]
    public void Create_Rejects_Unknown_Target_Empty_Actor_Or_Body()
    {
        Assert.Throws<ArgumentException>(() => Comment.Create(Guid.Empty, "Review", ReviewId, "B", Now));
        Assert.Throws<ArgumentException>(() => Comment.Create(Actor, "UserPhoto", ReviewId, "B", Now));
        Assert.Throws<ArgumentException>(() => Comment.Create(Actor, "Review", Guid.Empty, "B", Now));
        Assert.Throws<ArgumentException>(() => Comment.Create(Actor, "Review", ReviewId, "  ", Now));
    }

    [Fact]
    public void SetBody_Updates_Text()
    {
        var comment = Comment.Create(Actor, "Travelogue", ReviewId, "A", Now);
        var later = Instant.FromUtc(2026, 8, 17, 22, 0);
        comment.SetBody("Updated", later);
        Assert.Equal("Updated", comment.Body);
        Assert.Equal(later, comment.UpdatedAt);
        Assert.Equal(CommentTargetType.Travelogue, comment.TargetType);
    }
}
