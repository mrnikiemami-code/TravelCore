using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Shared UGC moderation/publication lifecycle (TC-P16-T007 / P16-R7).
/// </summary>
public sealed class UgcLifecycleTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 23, 0);
    private static readonly Instant Later = Instant.FromUtc(2026, 8, 17, 23, 30);

    [Fact]
    public void Review_Comment_UserPhoto_Enter_Pending_Directly()
    {
        var review = Review.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000091"),
            4,
            Now,
            "Place",
            Guid.Parse("0198b3e0-0000-7000-8000-000000000092"));
        var comment = Comment.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000093"),
            "Review",
            review.Id.Value,
            "Hi",
            Now);
        var photo = UserPhoto.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000094"),
            Guid.Parse("0198b3e0-0000-7000-8000-000000000095"),
            Now);

        Assert.Equal(ModerationStatus.Pending, review.ModerationStatus);
        Assert.Equal(PublicationStatus.Hidden, review.PublicationStatus);
        Assert.False(review.IsPubliclyEligible);
        Assert.Equal(ModerationStatus.Pending, comment.ModerationStatus);
        Assert.Equal(PublicationStatus.Hidden, comment.PublicationStatus);
        Assert.Equal(ModerationStatus.Pending, photo.ModerationStatus);
        Assert.Equal(PublicationStatus.Hidden, photo.PublicationStatus);
        Assert.True(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.False(UgcOwnershipBoundary.ApprovedEqualsPublished);
        Assert.False(UgcOwnershipBoundary.PublishedEqualsSeoIndexed);
    }

    [Fact]
    public void Public_Eligibility_Is_Approved_Plus_Published_Only()
    {
        var review = Review.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000096"),
            5,
            Now,
            "TourProduct",
            Guid.Parse("0198b3e0-0000-7000-8000-000000000097"));

        Assert.Throws<InvalidOperationException>(() => review.Publish(Later));
        review.Approve(Later);
        Assert.False(review.IsPubliclyEligible);
        Assert.Equal(ModerationStatus.Approved, review.ModerationStatus);
        Assert.Equal(PublicationStatus.Hidden, review.PublicationStatus);

        review.Publish(Later);
        Assert.True(review.IsPubliclyEligible);
        Assert.Equal(PublicationStatus.Published, review.PublicationStatus);

        review.Hide(Later);
        Assert.False(review.IsPubliclyEligible);
        Assert.Equal(PublicationStatus.Hidden, review.PublicationStatus);
        Assert.Equal(ModerationStatus.Approved, review.ModerationStatus);
    }

    [Fact]
    public void Rejected_Content_Is_Never_Publicly_Eligible()
    {
        var comment = Comment.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-000000000098"),
            "Travelogue",
            Guid.Parse("0198b3e0-0000-7000-8000-000000000099"),
            "Nope",
            Now);
        comment.Reject(Later);
        Assert.Equal(ModerationStatus.Rejected, comment.ModerationStatus);
        Assert.False(comment.IsPubliclyEligible);
        Assert.Throws<InvalidOperationException>(() => comment.Publish(Later));
    }

    [Fact]
    public void Travelogue_Starts_Draft_And_Can_Be_Approved_Without_Publishing()
    {
        var travelogue = Travelogue.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-00000000009a"),
            "fa",
            "Title",
            "Body",
            Now);

        Assert.Equal(PublicationStatus.Draft, travelogue.PublicationStatus);
        Assert.Equal(ModerationStatus.Pending, travelogue.ModerationStatus);
        Assert.False(travelogue.IsPubliclyEligible);

        travelogue.Submit(Later);
        travelogue.Approve(Later);
        Assert.Equal(ModerationStatus.Approved, travelogue.ModerationStatus);
        Assert.Equal(PublicationStatus.Draft, travelogue.PublicationStatus);
        Assert.False(travelogue.IsPubliclyEligible);

        travelogue.Publish(Later);
        Assert.True(travelogue.IsPubliclyEligible);
    }

    [Fact]
    public void Archive_Blocks_Further_Lifecycle_Changes()
    {
        var photo = UserPhoto.Create(
            Guid.Parse("0198b3e0-0000-7000-8000-00000000009b"),
            Guid.Parse("0198b3e0-0000-7000-8000-00000000009c"),
            Now);
        photo.Archive(Later);
        Assert.Equal(PublicationStatus.Archived, photo.PublicationStatus);
        Assert.Throws<InvalidOperationException>(() => photo.Approve(Later));
        Assert.Throws<InvalidOperationException>(() => photo.Publish(Later));
        Assert.Throws<InvalidOperationException>(() => photo.Hide(Later));
    }
}
