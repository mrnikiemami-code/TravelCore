using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Deterministic public UGC composition reads (TC-P16-T008 / P16-R8).
/// Approved + Published only. No Search engine, ranking, or IndexPolicy.
/// </summary>
internal sealed class UgcPublicQuery :
    IUgcPublicReviewQuery,
    IUgcPublicTravelogueQuery,
    IUgcPublicUserPhotoQuery,
    IUgcPublicCommentQuery
{
    private readonly UgcDbContext _db;

    public UgcPublicQuery(UgcDbContext db)
    {
        _db = db;
    }

    async Task<EligiblePublicReviewPage> IUgcPublicReviewQuery.GetByTargetAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        var target = ReviewTarget.Create(targetType, targetId);
        var rows = await _db.Reviews
            .AsNoTracking()
            .Where(x => x.TargetId == target.TargetId)
            .ToListAsync(cancellationToken);

        var eligible = rows
            .Where(x =>
                x.TargetType == target.TargetType
                && UgcPublicEligibility.IsPubliclyEligible(x.ModerationStatus.Value, x.PublicationStatus.Value))
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .ToList();

        var summary = new EligiblePublicRatingSummary(
            target.TargetType.Value,
            target.TargetId,
            eligible.Count,
            Average(eligible.Select(x => x.OverallRating.Value)));

        var pageItems = eligible.Take(UgcPublicEligibility.MaxReviews).ToList();
        var commentsByReview = await LoadCommentsByParentsAsync(
            CommentTargetType.Review,
            pageItems.Select(x => x.Id.Value).ToList(),
            cancellationToken);

        var items = pageItems
            .Select(review => MapReview(review, commentsByReview))
            .ToList();

        return new EligiblePublicReviewPage(summary, items);
    }

    public async Task<IReadOnlyList<EligiblePublicTravelogue>> ListByLocaleAsync(
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var locale = Travelogue.NormalizeLocaleCode(localeCode);
        var rows = await _db.Travelogues.AsNoTracking().ToListAsync(cancellationToken);
        var eligible = rows
            .Where(x =>
                string.Equals(x.LocaleCode, locale, StringComparison.OrdinalIgnoreCase)
                && UgcPublicEligibility.IsPubliclyEligible(x.ModerationStatus.Value, x.PublicationStatus.Value))
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .Take(UgcPublicEligibility.MaxTravelogues)
            .ToList();

        return await MapTraveloguesAsync(eligible, cancellationToken);
    }

    public async Task<EligiblePublicTravelogue?> GetByIdAsync(
        Guid travelogueId,
        CancellationToken cancellationToken = default)
    {
        if (travelogueId == Guid.Empty)
        {
            return null;
        }

        var row = await _db.Travelogues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Value == travelogueId, cancellationToken);
        if (row is null
            || !UgcPublicEligibility.IsPubliclyEligible(row.ModerationStatus.Value, row.PublicationStatus.Value))
        {
            return null;
        }

        var mapped = await MapTraveloguesAsync([row], cancellationToken);
        return mapped.Count == 0 ? null : mapped[0];
    }

    private async Task<IReadOnlyList<EligiblePublicTravelogue>> MapTraveloguesAsync(
        IReadOnlyList<Travelogue> eligible,
        CancellationToken cancellationToken)
    {
        var commentsByTravelogue = await LoadCommentsByParentsAsync(
            CommentTargetType.Travelogue,
            eligible.Select(x => x.Id.Value).ToList(),
            cancellationToken);

        return eligible
            .Select(item => new EligiblePublicTravelogue(
                item.Id.Value,
                item.ActorId,
                item.LocaleCode,
                item.Title,
                item.Body,
                commentsByTravelogue.GetValueOrDefault(item.Id.Value, []),
                ToUtc(item.CreatedAt)))
            .ToList();
    }

    public async Task<IReadOnlyList<EligiblePublicUserPhoto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.UserPhotos.AsNoTracking().ToListAsync(cancellationToken);
        return rows
            .Where(x => UgcPublicEligibility.IsPubliclyEligible(x.ModerationStatus.Value, x.PublicationStatus.Value))
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .Take(UgcPublicEligibility.MaxUserPhotos)
            .Select(x => new EligiblePublicUserPhoto(
                x.Id.Value,
                x.ActorId,
                x.MediaAssetId,
                ToUtc(x.CreatedAt)))
            .ToList();
    }

    async Task<IReadOnlyList<EligiblePublicComment>> IUgcPublicCommentQuery.GetByTargetAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        var target = CommentTarget.Create(targetType, targetId);
        var map = await LoadCommentsByParentsAsync(target.TargetType, [target.TargetId], cancellationToken);
        return map.GetValueOrDefault(target.TargetId, []);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<EligiblePublicComment>>> LoadCommentsByParentsAsync(
        CommentTargetType targetType,
        IReadOnlyList<Guid> parentIds,
        CancellationToken cancellationToken)
    {
        if (parentIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<EligiblePublicComment>>();
        }

        var idSet = parentIds.Distinct().ToList();
        var rows = await _db.Comments
            .AsNoTracking()
            .Where(x => idSet.Contains(x.TargetId))
            .ToListAsync(cancellationToken);

        return rows
            .Where(x =>
                x.TargetType == targetType
                && UgcPublicEligibility.IsPubliclyEligible(x.ModerationStatus.Value, x.PublicationStatus.Value))
            .GroupBy(x => x.TargetId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<EligiblePublicComment>)group
                    .OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id.Value)
                    .Take(UgcPublicEligibility.MaxComments)
                    .Select(MapComment)
                    .ToList());
    }

    private static EligiblePublicReview MapReview(
        Review review,
        IReadOnlyDictionary<Guid, IReadOnlyList<EligiblePublicComment>> commentsByReview)
    {
        var dimensions = review.DimensionRatings
            .OrderBy(x => x.DimensionCode.Value, StringComparer.Ordinal)
            .Select(x => new EligiblePublicDimensionRating(x.DimensionCode.Value, x.Value.Value))
            .ToList();

        return new EligiblePublicReview(
            review.Id.Value,
            review.ActorId,
            review.TargetType.Value,
            review.TargetId,
            review.OverallRating.Value,
            review.Title,
            review.Body,
            dimensions,
            commentsByReview.GetValueOrDefault(review.Id.Value, []),
            ToUtc(review.CreatedAt));
    }

    private static EligiblePublicComment MapComment(Comment comment) =>
        new(
            comment.Id.Value,
            comment.ActorId,
            comment.TargetType.Value,
            comment.TargetId,
            comment.Body,
            ToUtc(comment.CreatedAt));

    private static decimal Average(IEnumerable<int> values)
    {
        var list = values.ToList();
        if (list.Count == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)list.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static DateTimeOffset ToUtc(NodaTime.Instant instant) => instant.ToDateTimeOffset();
}
