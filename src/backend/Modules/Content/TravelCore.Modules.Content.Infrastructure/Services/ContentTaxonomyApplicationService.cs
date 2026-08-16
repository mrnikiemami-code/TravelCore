using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Content-owned Category/Tag catalog service (TC-P08-T004). Author omitted (P08-R7 open).
/// </summary>
public sealed class ContentTaxonomyApplicationService : IContentTaxonomyService
{
    private const int MaxListTake = 200;

    private readonly ContentDbContext _db;
    private readonly IClock _clock;

    public ContentTaxonomyApplicationService(ContentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ContentCategoryResponse> CreateCategoryAsync(
        CreateContentCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var category = ContentCategory.Create(request.Code, request.EnglishName, _clock.GetCurrentInstant());
        _db.ContentCategories.Add(category);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "ContentCategory persistence conflict (e.g. duplicate code).",
                ex);
        }

        return MapCategory(category);
    }

    public async Task<ContentCategoryResponse?> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var key = ContentCategoryId.From(id);
        var category = await _db.ContentCategories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
        return category is null ? null : MapCategory(category);
    }

    public async Task<IReadOnlyList<ContentCategoryResponse>> ListCategoriesAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        take = NormalizeTake(take);
        var items = await _db.ContentCategories.AsNoTracking()
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        return items.Select(MapCategory).ToList();
    }

    public async Task<ContentTagResponse> CreateTagAsync(
        CreateContentTagRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var tag = ContentTag.Create(request.Code, request.EnglishName, _clock.GetCurrentInstant());
        _db.ContentTags.Add(tag);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "ContentTag persistence conflict (e.g. duplicate code).",
                ex);
        }

        return MapTag(tag);
    }

    public async Task<ContentTagResponse?> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var key = ContentTagId.From(id);
        var tag = await _db.ContentTags.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
        return tag is null ? null : MapTag(tag);
    }

    public async Task<IReadOnlyList<ContentTagResponse>> ListTagsAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        take = NormalizeTake(take);
        var items = await _db.ContentTags.AsNoTracking()
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        return items.Select(MapTag).ToList();
    }

    private static int NormalizeTake(int take)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be positive.");
        }

        return Math.Min(take, MaxListTake);
    }

    private static ContentCategoryResponse MapCategory(ContentCategory category) =>
        new(
            category.Id.Value,
            category.Code,
            category.EnglishName,
            category.CreatedAt.ToString(),
            category.UpdatedAt.ToString());

    private static ContentTagResponse MapTag(ContentTag tag) =>
        new(
            tag.Id.Value,
            tag.Code,
            tag.EnglishName,
            tag.CreatedAt.ToString(),
            tag.UpdatedAt.ToString());
}
