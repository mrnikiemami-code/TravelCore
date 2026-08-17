using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Review aggregate + structured ratings (TC-P16-T002 / P16-R2). Rating is not an independent aggregate.
/// </summary>
public sealed class ReviewTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);

    [Fact]
    public void Create_Owns_OverallRating_And_Optional_Text()
    {
        var actorId = Guid.Parse("0198b3e0-0000-7000-8000-000000000031");
        var review = Review.Create(actorId, overallRating: 4, Now, title: "  Quiet stay  ", body: "Good.");

        Assert.NotEqual(Guid.Empty, review.Id.Value);
        Assert.Equal(actorId, review.ActorId);
        Assert.Equal(4, review.OverallRating.Value);
        Assert.Equal("Quiet stay", review.Title);
        Assert.Equal("Good.", review.Body);
        Assert.Equal(Now, review.CreatedAt);
        Assert.Equal(Now, review.UpdatedAt);
        Assert.Empty(review.DimensionRatings);
        Assert.True(UgcOwnershipBoundary.OverallRatingOwnedByReview);
        Assert.False(UgcOwnershipBoundary.RatingIsIndependentAggregate);
        Assert.Null(typeof(Review).GetProperty("TargetId"));
        Assert.Null(typeof(Review).GetProperty("TargetType"));
    }

    [Fact]
    public void RatingValue_Accepts_1_Through_5_And_Rejects_Outside()
    {
        Assert.Equal(1, RatingValue.From(1).Value);
        Assert.Equal(5, RatingValue.From(5).Value);
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingValue.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingValue.From(6));
        Assert.Throws<ArgumentOutOfRangeException>(() => Review.Create(Guid.NewGuid(), 0, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => Review.Create(Guid.NewGuid(), 6, Now));
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
        var review = Review.Create(Guid.NewGuid(), 5, Now);
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
        Assert.Throws<ArgumentException>(() => Review.Create(Guid.Empty, 3, Now));
        Assert.Throws<ArgumentException>(() => Review.Create(Guid.NewGuid(), 3, Now, title: new string('a', Review.TitleMaxLength + 1)));
        Assert.Throws<ArgumentException>(() => Review.Create(Guid.NewGuid(), 3, Now, body: new string('b', Review.BodyMaxLength + 1)));
    }
}
