using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Application service implementing ContentItem create/get/list (editorial SoR only).
/// </summary>
public sealed class ContentItemApplicationService : IContentItemService
{
    private const int MaxListTake = 200;

    private readonly ContentDbContext _db;
    private readonly IClock _clock;

    public ContentItemApplicationService(ContentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ContentItemResponse> CreateAsync(
        CreateContentItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var kind = ParseKind(request.Kind);
        var now = _clock.GetCurrentInstant();
        ContentItemAggregate item = kind switch
        {
            ContentKind.Article => ContentItemAggregate.CreateArticle(
                request.Code,
                request.EnglishName,
                now),
            ContentKind.LandingPage => ContentItemAggregate.CreateLandingPage(
                request.Code,
                request.EnglishName,
                now),
            ContentKind.Guide => ContentItemAggregate.CreateGuide(
                request.Code,
                request.EnglishName,
                now),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Kind), request.Kind, "Unsupported ContentKind.")
        };

        _db.ContentItems.Add(item);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "ContentItem persistence conflict (e.g. duplicate code).",
                ex);
        }

        return Map(item);
    }

    public async Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contentItemId = ContentItemId.From(id);
        var item = await _db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == contentItemId, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyList<ContentItemResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be positive.");
        }

        take = Math.Min(take, MaxListTake);
        var query = _db.ContentItems.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(kind))
        {
            var parsed = ParseKind(kind);
            query = query.Where(x => x.Kind == parsed);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    private static ContentKind ParseKind(string kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        if (Enum.TryParse<ContentKind>(kind.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported ContentKind '{kind}'.", nameof(kind));
    }

    private static ContentItemResponse Map(ContentItemAggregate item) =>
        new(
            item.Id.Value,
            item.Kind.ToString(),
            item.Code,
            item.EnglishName,
            item.Article is null ? null : new ArticleDetailsResponse(),
            item.LandingPage is null ? null : new LandingPageDetailsResponse(),
            item.Guide is null ? null : new GuideDetailsResponse(),
            item.CreatedAt.ToString(),
            item.UpdatedAt.ToString());
}
