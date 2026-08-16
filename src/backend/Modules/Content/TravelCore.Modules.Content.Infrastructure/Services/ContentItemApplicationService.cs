using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.ReferenceData.Contracts;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Application service implementing ContentItem create/get/list + localization + taxonomy links.
/// </summary>
public sealed class ContentItemApplicationService : IContentItemService
{
    private const int MaxListTake = 200;

    private readonly ContentDbContext _db;
    private readonly IClock _clock;
    private readonly IReferenceDataCatalogQuery _referenceData;
    private readonly IDestinationExistenceQuery _destinations;

    public ContentItemApplicationService(
        ContentDbContext db,
        IClock clock,
        IReferenceDataCatalogQuery referenceData,
        IDestinationExistenceQuery destinations)
    {
        _db = db;
        _clock = clock;
        _referenceData = referenceData;
        _destinations = destinations;
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

    public Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        GetByIdAsync(id, locale: null, cancellationToken);

    public async Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken = default)
    {
        var contentItemId = ContentItemId.From(id);
        var item = await _db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == contentItemId, cancellationToken);
        return item is null ? null : Map(item, locale);
    }

    public async Task<ContentItemResponse?> GetByCodeAsync(
        string code,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = ContentItemAggregate.NormalizeCode(code);
        var item = await _db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
        return item is null ? null : Map(item, locale);
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

        return items.Select(x => Map(x)).ToList();
    }

    public async Task<ContentItemTranslationResponse> UpsertTranslationAsync(
        Guid contentItemId,
        string localeCode,
        UpsertContentItemTranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var locale = await _referenceData.GetLocaleAsync(localeCode, cancellationToken)
            ?? throw new ArgumentException(
                $"Locale '{localeCode}' was not found in ReferenceData locale catalog.",
                nameof(localeCode));

        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var translation = item.UpsertTranslation(
            locale.Code,
            request.Title,
            request.Body,
            request.Excerpt,
            now);

        await _db.SaveChangesAsync(cancellationToken);
        return MapTranslation(translation);
    }

    public async Task<ContentSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        bool publicOnly = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedLocale = ContentItemTranslation.NormalizeLocaleCode(localeCode);
        var normalizedSlug = ContentItemTranslation.NormalizeSlug(slug)
            ?? throw new ArgumentException("Slug is required.", nameof(slug));

        var hit = await _db.ContentItems.AsNoTracking()
            .SelectMany(p => p.Translations.Select(t => new { Item = p, Translation = t }))
            .FirstOrDefaultAsync(
                x => x.Translation.LocaleCode == normalizedLocale && x.Translation.Slug == normalizedSlug,
                cancellationToken);

        if (hit is null)
        {
            return null;
        }

        // P08-R3/R4: no CatalogStatus invent — public gate = locale translation with title + slug.
        if (publicOnly
            && (string.IsNullOrWhiteSpace(hit.Translation.Title)
                || string.IsNullOrWhiteSpace(hit.Translation.Slug)))
        {
            return null;
        }

        return new ContentSlugLookupResponse(
            hit.Item.Id.Value,
            hit.Translation.LocaleCode,
            hit.Translation.Slug!,
            hit.Item.Kind.ToString(),
            hit.Item.Code,
            hit.Item.EnglishName);
    }

    public async Task<ContentItemTranslationResponse> SetTranslationSlugAsync(
        Guid contentItemId,
        string localeCode,
        SetContentItemTranslationSlugRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var locale = await _referenceData.GetLocaleAsync(localeCode, cancellationToken)
            ?? throw new ArgumentException(
                $"Locale '{localeCode}' was not found in ReferenceData locale catalog.",
                nameof(localeCode));

        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var translation = item.SetTranslationSlug(locale.Code, request.Slug, now);
        if (translation.Slug is not null)
        {
            await EnsureSlugUniqueAsync(locale.Code, translation.Slug, item.Id, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return MapTranslation(translation);
    }

    public async Task<IReadOnlyList<ContentItemTranslationResponse>> ListTranslationsAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default)
    {
        var id = ContentItemId.From(contentItemId);
        var item = await _db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return [];
        }

        return item.Translations
            .OrderBy(x => x.LocaleCode, StringComparer.Ordinal)
            .Select(MapTranslation)
            .ToList();
    }

    public async Task<ContentItemResponse> AssignCategoryAsync(
        Guid contentItemId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryKey = ContentCategoryId.From(categoryId);
        var categoryExists = await _db.ContentCategories.AsNoTracking()
            .AnyAsync(x => x.Id == categoryKey, cancellationToken);
        if (!categoryExists)
        {
            throw new KeyNotFoundException($"ContentCategory '{categoryId}' was not found.");
        }

        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.AssignCategory(categoryKey, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentItemResponse> RemoveCategoryAsync(
        Guid contentItemId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.RemoveCategory(ContentCategoryId.From(categoryId), _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentItemResponse> AssignTagAsync(
        Guid contentItemId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        var tagKey = ContentTagId.From(tagId);
        var tagExists = await _db.ContentTags.AsNoTracking()
            .AnyAsync(x => x.Id == tagKey, cancellationToken);
        if (!tagExists)
        {
            throw new KeyNotFoundException($"ContentTag '{tagId}' was not found.");
        }

        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.AssignTag(tagKey, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentItemResponse> RemoveTagAsync(
        Guid contentItemId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.RemoveTag(ContentTagId.From(tagId), _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentItemResponse> AssignDestinationAsync(
        Guid contentItemId,
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDestinationExistsAsync(destinationId, cancellationToken);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.AssignDestination(destinationId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentItemResponse> RemoveDestinationAsync(
        Guid contentItemId,
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        item.RemoveDestination(destinationId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    private async Task EnsureSlugUniqueAsync(
        string localeCode,
        string slug,
        ContentItemId ownerId,
        CancellationToken cancellationToken)
    {
        var conflict = await _db.ContentItems.AsNoTracking()
            .SelectMany(p => p.Translations.Select(t => new { ContentItemId = p.Id, t.LocaleCode, t.Slug }))
            .FirstOrDefaultAsync(
                x => x.LocaleCode == localeCode && x.Slug == slug && x.ContentItemId != ownerId,
                cancellationToken);

        if (conflict is not null)
        {
            throw new ArgumentException(
                $"Slug '{slug}' is already used for locale '{localeCode}'.",
                nameof(slug));
        }
    }

    private async Task EnsureDestinationExistsAsync(
        Guid destinationId,
        CancellationToken cancellationToken)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        if (!await _destinations.ExistsAsync(destinationId, cancellationToken))
        {
            throw new ArgumentException(
                $"Destination '{destinationId}' was not found.",
                nameof(destinationId));
        }
    }

    private async Task<ContentItemAggregate> LoadTrackedAsync(
        Guid contentItemId,
        CancellationToken cancellationToken)
    {
        var id = ContentItemId.From(contentItemId);
        var item = await _db.ContentItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item
            ?? throw new KeyNotFoundException($"ContentItem '{contentItemId}' was not found.");
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

    private static ContentItemResponse Map(ContentItemAggregate item, string? locale = null)
    {
        string? localizedTitle = null;
        string? localizedBody = null;
        string? localizedExcerpt = null;

        // ADR 0008: exact-locale overlay only — no silent cross-language invent.
        if (!string.IsNullOrWhiteSpace(locale))
        {
            var translation = item.FindTranslation(locale);
            if (translation is not null)
            {
                localizedTitle = translation.Title;
                localizedBody = translation.Body;
                localizedExcerpt = translation.Excerpt;
            }
        }

        return new ContentItemResponse(
            item.Id.Value,
            item.Kind.ToString(),
            item.Code,
            item.EnglishName,
            item.Article is null ? null : new ArticleDetailsResponse(),
            item.LandingPage is null ? null : new LandingPageDetailsResponse(),
            item.Guide is null ? null : new GuideDetailsResponse(),
            item.CreatedAt.ToString(),
            item.UpdatedAt.ToString(),
            localizedTitle,
            localizedBody,
            localizedExcerpt,
            item.Categories.Select(x => x.CategoryId.Value).OrderBy(x => x).ToList(),
            item.Tags.Select(x => x.TagId.Value).OrderBy(x => x).ToList(),
            item.Destinations.Select(x => x.DestinationId).OrderBy(x => x).ToList());
    }

    private static ContentItemTranslationResponse MapTranslation(ContentItemTranslation translation) =>
        new(
            translation.ContentItemId.Value,
            translation.LocaleCode,
            translation.Title,
            translation.Body,
            translation.Excerpt,
            translation.Slug,
            translation.UpdatedAt.ToString());
}
