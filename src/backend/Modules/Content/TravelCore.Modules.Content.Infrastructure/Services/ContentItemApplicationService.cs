using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;
using TravelCore.Modules.ReferenceData.Contracts;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Application service implementing ContentItem create/get/list + localization (editorial SoR only).
/// </summary>
public sealed class ContentItemApplicationService : IContentItemService
{
    private const int MaxListTake = 200;

    private readonly ContentDbContext _db;
    private readonly IClock _clock;
    private readonly IReferenceDataCatalogQuery _referenceData;

    public ContentItemApplicationService(
        ContentDbContext db,
        IClock clock,
        IReferenceDataCatalogQuery referenceData)
    {
        _db = db;
        _clock = clock;
        _referenceData = referenceData;
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
            localizedExcerpt);
    }

    private static ContentItemTranslationResponse MapTranslation(ContentItemTranslation translation) =>
        new(
            translation.ContentItemId.Value,
            translation.LocaleCode,
            translation.Title,
            translation.Body,
            translation.Excerpt,
            translation.UpdatedAt.ToString());
}
