namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Structured child rating on a Review. No independent lifecycle (P16-R2).
/// </summary>
public sealed class ReviewDimensionRating
{
    private ReviewDimensionRating()
    {
        DimensionCode = default;
    }

    internal ReviewDimensionRating(ReviewId reviewId, ReviewDimensionCode dimensionCode, RatingValue value)
    {
        if (reviewId.Value == Guid.Empty)
        {
            throw new ArgumentException("ReviewId cannot be empty.", nameof(reviewId));
        }

        ReviewId = reviewId;
        DimensionCode = dimensionCode;
        Value = value;
    }

    public ReviewId ReviewId { get; private set; }

    public ReviewDimensionCode DimensionCode { get; private set; }

    public RatingValue Value { get; private set; }

    internal void ReplaceValue(RatingValue value) => Value = value;
}
