using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Review aggregate + structured ratings + logical target (TC-P16-T002 / T003).
/// </summary>
public sealed class ReviewTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid PlaceTarget = Guid.Parse("0198b3e0-0000-7000-8000-000000000041");

    private static Review CreateReview(
        Guid? actorId = null,
        int overallRating = 4,
        string targetType = ReviewTargetType.PlaceValue,
        Guid? targetId = null,
        string? title = null,
        string? body = null) =>
        Review.Create(
            actorId ?? Guid.Parse("0198b3e0-0000-7000-8000-000000000031"),
            overallRating,
            Now,
            targetType,
            targetId ?? PlaceTarget,
            title,
            body);

    [Fact]
    public void Create_Owns_OverallRating_Optional_Text_And_Exactly_One_Target()
    {
        var actorId = Guid.Parse("0198b3e0-0000-7000-8000-000000000031");
        var review = CreateReview(actorId, 4, ReviewTargetType.PlaceValue, PlaceTarget, "  Quiet stay  ", "Good.");

        Assert.NotEqual(Guid.Empty, review.Id.Value);
        Assert.Equal(actorId, review.ActorId);
        Assert.Equal(4, review.OverallRating.Value);
        Assert.Equal("Quiet stay", review.Title);
        Assert.Equal("Good.", review.Body);
        Assert.Equal(ReviewTargetType.Place, review.TargetType);
        Assert.Equal(PlaceTarget, review.TargetId);
        Assert.Equal(PlaceTarget, review.Target.TargetId);
        Assert.Equal(Now, review.CreatedAt);
        Assert.Equal(Now, review.UpdatedAt);
        Assert.Empty(review.DimensionRatings);
        Assert.True(UgcOwnershipBoundary.OverallRatingOwnedByReview);
        Assert.False(UgcOwnershipBoundary.RatingIsIndependentAggregate);
        Assert.True(UgcOwnershipBoundary.TargetAttachmentModelCommitted);
        Assert.True(UgcOwnershipBoundary.ReviewTargetIsLogicalReferenceOnly);
        Assert.False(UgcOwnershipBoundary.OwnsTargetFacts);
    }

    [Fact]
    public void RatingValue_Accepts_1_Through_5_And_Rejects_Outside()
    {
        Assert.Equal(1, RatingValue.From(1).Value);
        Assert.Equal(5, RatingValue.From(5).Value);
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingValue.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingValue.From(6));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateReview(overallRating: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateReview(overallRating: 6));
    }

    [Fact]
    public void DimensionCode_Normalizes_And_Rejects_Invalid()
    {
        Assert.Equal("service", ReviewDimensionCode.Parse("  Service  ").Value);
        Assert.Equal("food_quality", ReviewDimensionCode.Parse("FOOD_QUALITY").Value);
        Assert.Equal("hotelrating", ReviewDimensionCode.Parse("HotelRating").Value);
        Assert.Throws<ArgumentException>(() => ReviewDimensionCode.Parse("1food"));
        Assert.Throws<ArgumentException>(() => ReviewDimensionCode.Parse("food-quality"));
        Assert.Throws<ArgumentException>(() => ReviewDimensionCode.Parse(""));
    }

    [Fact]
    public void UpsertDimensionRating_Is_Unique_Per_Review_And_Child_Only()
    {
        var review = CreateReview(overallRating: 5);
        review.UpsertDimensionRating("Service", 3, Now);
        review.UpsertDimensionRating("service", 4, Instant.FromUtc(2026, 8, 17, 22, 0));
        review.UpsertDimensionRating("food", 5, Instant.FromUtc(2026, 8, 17, 22, 0));

        Assert.Equal(2, review.DimensionRatings.Count);
        var service = Assert.Single(review.DimensionRatings, x => x.DimensionCode.Value == "service");
        Assert.Equal(4, service.Value.Value);
        Assert.Equal(review.Id, service.ReviewId);
        Assert.True(review.RemoveDimensionRating("FOOD", Instant.FromUtc(2026, 8, 17, 23, 0)));
        Assert.Single(review.DimensionRatings);
        Assert.False(review.RemoveDimensionRating("missing", Now));
        Assert.Null(typeof(ReviewDimensionRating).Assembly.GetType("TravelCore.Modules.Ugc.Domain.Rating"));
    }

    [Fact]
    public void Create_Rejects_Empty_Actor_And_Oversized_Text()
    {
        Assert.Throws<ArgumentException>(() => CreateReview(actorId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateReview(title: new string('a', Review.TitleMaxLength + 1)));
        Assert.Throws<ArgumentException>(() => CreateReview(body: new string('b', Review.BodyMaxLength + 1)));
    }

    [Fact]
    public void Target_Must_Be_Supported_Type_With_Non_Empty_Id()
    {
        Assert.Equal(ReviewTargetType.TourProduct, ReviewTargetType.Parse("TourProduct"));
        Assert.Equal(ReviewTargetType.Agency, ReviewTargetType.Parse("Agency"));
        Assert.Throws<ArgumentException>(() => ReviewTargetType.Parse("Hotel"));
        Assert.Throws<ArgumentException>(() => ReviewTargetType.Parse("Destination"));
        Assert.Throws<ArgumentException>(() => ReviewTargetType.Parse("tourproduct"));
        Assert.Throws<ArgumentException>(() => ReviewTarget.Create(ReviewTargetType.PlaceValue, Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateReview(targetType: "Hotel"));
        Assert.Throws<ArgumentException>(() => CreateReview(targetId: Guid.Empty));
    }

    [Fact]
    public void SetTarget_Replaces_Logical_Reference_And_Rejects_Invalid()
    {
        var review = CreateReview();
        var agencyId = Guid.Parse("0198b3e0-0000-7000-8000-000000000051");
        var later = Instant.FromUtc(2026, 8, 17, 22, 0);

        review.SetTarget(ReviewTargetType.AgencyValue, agencyId, later);

        Assert.Equal(ReviewTargetType.Agency, review.TargetType);
        Assert.Equal(agencyId, review.TargetId);
        Assert.Equal(later, review.UpdatedAt);
        Assert.Throws<ArgumentException>(() => review.SetTarget("Tour", Guid.NewGuid(), later));
        Assert.Throws<ArgumentException>(() => review.SetTarget(ReviewTargetType.PlaceValue, Guid.Empty, later));
    }
}
