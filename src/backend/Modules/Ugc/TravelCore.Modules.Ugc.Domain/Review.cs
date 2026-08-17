using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// User-authored feedback aggregate (TC-P16-T002 / P16-R2).
/// OverallRating is part of Review. Dimension ratings are children. Rating is not an independent aggregate.
/// Target attachment is out of scope (P16-R3 UNRESOLVED).
/// </summary>
public sealed class Review
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 8000;

    private readonly List<ReviewDimensionRating> _dimensionRatings = [];

    private Review()
    {
    }

    private Review(
        ReviewId id,
        Guid actorId,
        RatingValue overallRating,
        string? title,
        string? body,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ReviewId cannot be empty.", nameof(id));
        }

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("ActorId cannot be empty.", nameof(actorId));
        }

        Id = id;
        ActorId = actorId;
        OverallRating = overallRating;
        Title = NormalizeOptional(title, TitleMaxLength, nameof(title));
        Body = NormalizeOptional(body, BodyMaxLength, nameof(body));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public ReviewId Id { get; private set; }

    /// <summary>Opaque logical actor id. Not Identity/Party ownership.</summary>
    public Guid ActorId { get; private set; }

    public RatingValue OverallRating { get; private set; }

    public string? Title { get; private set; }

    public string? Body { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyCollection<ReviewDimensionRating> DimensionRatings => _dimensionRatings;

    public static Review Create(
        Guid actorId,
        int overallRating,
        Instant now,
        string? title = null,
        string? body = null,
        IReadOnlyDictionary<string, int>? dimensionRatings = null)
    {
        var review = new Review(
            ReviewId.New(),
            actorId,
            RatingValue.From(overallRating),
            title,
            body,
            now);

        if (dimensionRatings is not null)
        {
            foreach (var pair in dimensionRatings)
            {
                review.UpsertDimensionRating(pair.Key, pair.Value, now, touch: false);
            }

            review.UpdatedAt = now;
        }

        return review;
    }

    public void SetOverallRating(int overallRating, Instant now)
    {
        OverallRating = RatingValue.From(overallRating);
        Touch(now);
    }

    public void SetText(string? title, string? body, Instant now)
    {
        Title = NormalizeOptional(title, TitleMaxLength, nameof(title));
        Body = NormalizeOptional(body, BodyMaxLength, nameof(body));
        Touch(now);
    }

    public ReviewDimensionRating UpsertDimensionRating(string dimensionCode, int value, Instant now)
        => UpsertDimensionRating(dimensionCode, value, now, touch: true);

    public bool RemoveDimensionRating(string dimensionCode, Instant now)
    {
        var code = ReviewDimensionCode.Parse(dimensionCode);
        var existing = _dimensionRatings.Find(x => x.DimensionCode.Value == code.Value);
        if (existing is null)
        {
            return false;
        }

        _dimensionRatings.Remove(existing);
        Touch(now);
        return true;
    }

    private ReviewDimensionRating UpsertDimensionRating(string dimensionCode, int value, Instant now, bool touch)
    {
        var code = ReviewDimensionCode.Parse(dimensionCode);
        var rating = RatingValue.From(value);
        var existing = _dimensionRatings.Find(x => x.DimensionCode.Value == code.Value);
        if (existing is not null)
        {
            existing.ReplaceValue(rating);
        }
        else
        {
            existing = new ReviewDimensionRating(Id, code, rating);
            _dimensionRatings.Add(existing);
        }

        if (touch)
        {
            Touch(now);
        }

        return existing;
    }

    private void Touch(Instant now) => UpdatedAt = now;

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }

        return trimmed;
    }
}
