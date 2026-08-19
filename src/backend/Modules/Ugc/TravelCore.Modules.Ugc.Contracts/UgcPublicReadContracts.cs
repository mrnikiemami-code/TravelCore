namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// Engine-neutral public UGC read (TC-P16-T008 / P16-R8).
/// UGC owns public-eligibility truth. Consumers must not query UGC persistence directly.
/// Publicly Eligible != SEO Indexed and != automatically Search indexed.
/// </summary>
public static class UgcPublicEligibility
{
    public const int MaxReviews = 6;
    public const int MaxTravelogues = 6;
    public const int MaxUserPhotos = 6;
    public const int MaxComments = 6;

    public static bool IsPubliclyEligible(string? moderationStatus, string? publicationStatus)
        => string.Equals(moderationStatus, "Approved", StringComparison.Ordinal)
           && string.Equals(publicationStatus, "Published", StringComparison.Ordinal);
}

/// <summary>Derived, rebuildable rating summary. Not an independent Average Rating engine.</summary>
public sealed record EligiblePublicRatingSummary(
    string TargetType,
    Guid TargetId,
    int EligibleReviewCount,
    decimal AverageOverallRating);

public sealed record EligiblePublicDimensionRating(string DimensionCode, int Value);

public sealed record EligiblePublicComment(
    Guid CommentId,
    Guid ActorId,
    string TargetType,
    Guid TargetId,
    string Body,
    DateTimeOffset CreatedAt);

public sealed record EligiblePublicReview(
    Guid ReviewId,
    Guid ActorId,
    string TargetType,
    Guid TargetId,
    int OverallRating,
    string? Title,
    string? Body,
    IReadOnlyList<EligiblePublicDimensionRating> DimensionRatings,
    IReadOnlyList<EligiblePublicComment> Comments,
    DateTimeOffset CreatedAt);

public sealed record EligiblePublicReviewPage(
    EligiblePublicRatingSummary Summary,
    IReadOnlyList<EligiblePublicReview> Items);

public sealed record EligiblePublicTravelogue(
    Guid TravelogueId,
    Guid ActorId,
    string LocaleCode,
    string Title,
    string Body,
    IReadOnlyList<EligiblePublicComment> Comments,
    DateTimeOffset CreatedAt);

public sealed record EligiblePublicUserPhoto(
    Guid UserPhotoId,
    Guid ActorId,
    Guid MediaAssetId,
    DateTimeOffset CreatedAt);

public interface IUgcPublicReviewQuery
{
    Task<EligiblePublicReviewPage> GetByTargetAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);
}

public interface IUgcPublicTravelogueQuery
{
    Task<IReadOnlyList<EligiblePublicTravelogue>> ListByLocaleAsync(
        string localeCode,
        CancellationToken cancellationToken = default);

    Task<EligiblePublicTravelogue?> GetByIdAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default);
}

public interface IUgcPublicUserPhotoQuery
{
    Task<IReadOnlyList<EligiblePublicUserPhoto>> ListAsync(
        CancellationToken cancellationToken = default);
}

public interface IUgcPublicCommentQuery
{
    Task<IReadOnlyList<EligiblePublicComment>> GetByTargetAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);
}
