using NodaTime;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Content editorial aggregate root (P08-R1).
/// Shared editorial facts live here; type-specific facts live on Article/LandingPage/Guide rows (1:1).
/// Localized title/body/excerpt live on ContentItemTranslation rows (T003; ADR 0008).
/// Category/Tag taxonomy links are Content-owned (T004). Author deferred (P08-R7 open).
/// Content Blocks are relational first-class entities (T005 / P08-R2). Widgets deferred (P08-R6).
/// Destination links are logical 0..N refs (T006 / P08-R5). Slug/SEO/Author/delete deferred (R3/R4/R7/R8).
/// </summary>
public sealed class ContentItem
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int MaxCategories = 32;
    public const int MaxTags = 64;
    public const int MaxBlocks = 200;

    private readonly List<ContentItemTranslation> _translations = [];
    private readonly List<ContentItemCategory> _categories = [];
    private readonly List<ContentItemTag> _tags = [];
    private readonly List<ContentBlock> _blocks = [];
    private readonly List<ContentItemDestination> _destinations = [];

    private ContentItem()
    {
        Code = null!;
        EnglishName = null!;
    }

    private ContentItem(
        ContentItemId id,
        ContentKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        Id = id;
        Kind = kind;
        Code = code;
        EnglishName = englishName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Article = article;
        LandingPage = landingPage;
        Guide = guide;
        ValidateSpecializationInvariant(id, kind, article, landingPage, guide);
    }

    public ContentItemId Id { get; private set; }

    public ContentKind Kind { get; private set; }

    /// <summary>Stable opaque content code within TravelCore (not SEO slug).</summary>
    public string Code { get; private set; }

    /// <summary>Baseline English display name (localized titles live in translations).</summary>
    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Article? Article { get; private set; }

    public LandingPage? LandingPage { get; private set; }

    public Guide? Guide { get; private set; }

    public IReadOnlyCollection<ContentItemTranslation> Translations => _translations;

    public IReadOnlyCollection<ContentItemCategory> Categories => _categories;

    public IReadOnlyCollection<ContentItemTag> Tags => _tags;

    public IReadOnlyCollection<ContentBlock> Blocks => _blocks;

    public IReadOnlyList<ContentBlock> BlocksOrdered =>
        _blocks.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToList();

    public IReadOnlyCollection<ContentItemDestination> Destinations => _destinations;

    public static ContentItem CreateArticle(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.Article,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: Domain.Article.Create(contentItemId),
            landingPage: null,
            guide: null);
    }

    public static ContentItem CreateLandingPage(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.LandingPage,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: null,
            landingPage: Domain.LandingPage.Create(contentItemId),
            guide: null);
    }

    public static ContentItem CreateGuide(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.Guide,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: null,
            landingPage: null,
            guide: Domain.Guide.Create(contentItemId));
    }

    /// <summary>
    /// Reconstitute a ContentItem with explicit specializations (tests / guarded composition).
    /// </summary>
    public static ContentItem Reconstitute(
        ContentItemId id,
        ContentKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Instant updatedAt,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        var item = new ContentItem(
            id,
            kind,
            NormalizeCode(code),
            NormalizeName(englishName),
            createdAt,
            article,
            landingPage,
            guide)
        {
            UpdatedAt = updatedAt
        };
        return item;
    }

    public ContentItemTranslation UpsertTranslation(
        string localeCode,
        string title,
        string? body,
        string? excerpt,
        Instant now)
    {
        var normalizedLocale = ContentItemTranslation.NormalizeLocaleCode(localeCode);
        var existing = _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));

        if (existing is null)
        {
            var created = ContentItemTranslation.Create(
                Id,
                normalizedLocale,
                title,
                body,
                excerpt,
                now);
            _translations.Add(created);
            UpdatedAt = now;
            return created;
        }

        existing.Update(title, body, excerpt, now);
        UpdatedAt = now;
        return existing;
    }

    public ContentItemTranslation? FindTranslation(string localeCode)
    {
        var normalizedLocale = ContentItemTranslation.NormalizeLocaleCode(localeCode);
        return _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));
    }

    public ContentItemCategory AssignCategory(ContentCategoryId categoryId, Instant now)
    {
        if (categoryId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentCategoryId cannot be empty.", nameof(categoryId));
        }

        var existing = _categories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (existing is not null)
        {
            return existing;
        }

        if (_categories.Count >= MaxCategories)
        {
            throw new InvalidOperationException($"A ContentItem may have at most {MaxCategories} categories.");
        }

        var link = ContentItemCategory.Create(Id, categoryId);
        _categories.Add(link);
        UpdatedAt = now;
        return link;
    }

    public bool RemoveCategory(ContentCategoryId categoryId, Instant now)
    {
        var existing = _categories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (existing is null)
        {
            return false;
        }

        _categories.Remove(existing);
        UpdatedAt = now;
        return true;
    }

    public ContentItemTag AssignTag(ContentTagId tagId, Instant now)
    {
        if (tagId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentTagId cannot be empty.", nameof(tagId));
        }

        var existing = _tags.FirstOrDefault(x => x.TagId == tagId);
        if (existing is not null)
        {
            return existing;
        }

        if (_tags.Count >= MaxTags)
        {
            throw new InvalidOperationException($"A ContentItem may have at most {MaxTags} tags.");
        }

        var link = ContentItemTag.Create(Id, tagId);
        _tags.Add(link);
        UpdatedAt = now;
        return link;
    }

    public bool RemoveTag(ContentTagId tagId, Instant now)
    {
        var existing = _tags.FirstOrDefault(x => x.TagId == tagId);
        if (existing is null)
        {
            return false;
        }

        _tags.Remove(existing);
        UpdatedAt = now;
        return true;
    }

    public ContentBlock AddHeadingBlock(string text, short level, Instant now, int? sortOrder = null)
    {
        var block = ContentBlock.CreateHeading(Id, ResolveInsertSortOrder(sortOrder), text, level);
        return AttachBlock(block, now);
    }

    public ContentBlock AddParagraphBlock(string text, Instant now, int? sortOrder = null)
    {
        var block = ContentBlock.CreateParagraph(Id, ResolveInsertSortOrder(sortOrder), text);
        return AttachBlock(block, now);
    }

    public ContentBlock AddImageBlock(Guid mediaAssetId, Instant now, string? caption = null, int? sortOrder = null)
    {
        var block = ContentBlock.CreateImage(Id, ResolveInsertSortOrder(sortOrder), mediaAssetId, caption);
        return AttachBlock(block, now);
    }

    public ContentBlock AddGalleryBlock(IReadOnlyList<Guid> mediaAssetIds, Instant now, int? sortOrder = null)
    {
        var block = ContentBlock.CreateGallery(Id, ResolveInsertSortOrder(sortOrder), mediaAssetIds);
        return AttachBlock(block, now);
    }

    public ContentBlock AddFaqBlock(
        IReadOnlyList<(string Question, string Answer)> items,
        Instant now,
        int? sortOrder = null)
    {
        var block = ContentBlock.CreateFaq(Id, ResolveInsertSortOrder(sortOrder), items);
        return AttachBlock(block, now);
    }

    public ContentBlock AddTableBlock(string text, Instant now, int? sortOrder = null)
    {
        var block = ContentBlock.CreateTable(Id, ResolveInsertSortOrder(sortOrder), text);
        return AttachBlock(block, now);
    }

    public ContentBlock AddVideoBlock(Guid mediaAssetId, Instant now, string? caption = null, int? sortOrder = null)
    {
        var block = ContentBlock.CreateVideo(Id, ResolveInsertSortOrder(sortOrder), mediaAssetId, caption);
        return AttachBlock(block, now);
    }

    public ContentBlock AddCtaBlock(string label, string href, Instant now, int? sortOrder = null)
    {
        var block = ContentBlock.CreateCta(Id, ResolveInsertSortOrder(sortOrder), label, href);
        return AttachBlock(block, now);
    }

    public bool RemoveBlock(ContentBlockId blockId, Instant now)
    {
        var existing = _blocks.FirstOrDefault(x => x.Id == blockId);
        if (existing is null)
        {
            return false;
        }

        _blocks.Remove(existing);
        CompactBlockSortOrders();
        UpdatedAt = now;
        return true;
    }

    public IReadOnlyList<ContentBlock> ReorderBlocks(IReadOnlyList<ContentBlockId> orderedBlockIds, Instant now)
    {
        ArgumentNullException.ThrowIfNull(orderedBlockIds);
        if (orderedBlockIds.Count != _blocks.Count)
        {
            throw new ArgumentException(
                "Reorder must include every existing block exactly once.",
                nameof(orderedBlockIds));
        }

        if (orderedBlockIds.Distinct().Count() != orderedBlockIds.Count)
        {
            throw new ArgumentException("Reorder ids must be unique.", nameof(orderedBlockIds));
        }

        var byId = _blocks.ToDictionary(x => x.Id);
        for (var i = 0; i < orderedBlockIds.Count; i++)
        {
            if (!byId.TryGetValue(orderedBlockIds[i], out var block))
            {
                throw new ArgumentException(
                    $"Unknown ContentBlockId '{orderedBlockIds[i]}'.",
                    nameof(orderedBlockIds));
            }

            block.SetSortOrder(i);
        }

        UpdatedAt = now;
        return BlocksOrdered;
    }

    public ContentItemDestination AssignDestination(Guid destinationId, Instant now)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        var existing = _destinations.FirstOrDefault(x => x.DestinationId == destinationId);
        if (existing is not null)
        {
            return existing;
        }

        if (_destinations.Count >= ContentItemDestination.MaxLinksPerContentItem)
        {
            throw new InvalidOperationException(
                $"A ContentItem may have at most {ContentItemDestination.MaxLinksPerContentItem} Destination links.");
        }

        var link = ContentItemDestination.Create(Id, destinationId);
        _destinations.Add(link);
        UpdatedAt = now;
        return link;
    }

    public bool RemoveDestination(Guid destinationId, Instant now)
    {
        var existing = _destinations.FirstOrDefault(x => x.DestinationId == destinationId);
        if (existing is null)
        {
            return false;
        }

        _destinations.Remove(existing);
        UpdatedAt = now;
        return true;
    }

    private ContentBlock AttachBlock(ContentBlock block, Instant now)
    {
        if (_blocks.Count >= MaxBlocks)
        {
            throw new InvalidOperationException($"A ContentItem may have at most {MaxBlocks} blocks.");
        }

        _blocks.Add(block);
        UpdatedAt = now;
        return block;
    }

    private int ResolveInsertSortOrder(int? sortOrder)
    {
        if (sortOrder is not null)
        {
            return sortOrder.Value;
        }

        return _blocks.Count == 0 ? 0 : _blocks.Max(x => x.SortOrder) + 1;
    }

    private void CompactBlockSortOrders()
    {
        var ordered = _blocks.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
    }

    /// <summary>
    /// Kind must match exactly one specialization row; multi-kind and mismatch are rejected.
    /// </summary>
    public static void ValidateSpecializationInvariant(
        ContentItemId contentItemId,
        ContentKind kind,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported ContentKind.");
        }

        var specializationCount =
            (article is null ? 0 : 1)
            + (landingPage is null ? 0 : 1)
            + (guide is null ? 0 : 1);

        if (specializationCount > 1)
        {
            throw new ArgumentException(
                "A ContentItem may have only one typed specialization (Article, LandingPage, or Guide).",
                nameof(kind));
        }

        if (specializationCount == 0)
        {
            throw new ArgumentException(
                "A ContentItem requires exactly one typed specialization matching its ContentKind.",
                nameof(kind));
        }

        switch (kind)
        {
            case ContentKind.Article:
                if (article is null)
                {
                    throw new ArgumentException(
                        "ContentKind.Article requires an Article specialization.",
                        nameof(article));
                }

                if (landingPage is not null || guide is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.Article cannot carry LandingPage or Guide specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, article.ContentItemId, nameof(article));
                break;

            case ContentKind.LandingPage:
                if (landingPage is null)
                {
                    throw new ArgumentException(
                        "ContentKind.LandingPage requires a LandingPage specialization.",
                        nameof(landingPage));
                }

                if (article is not null || guide is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.LandingPage cannot carry Article or Guide specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, landingPage.ContentItemId, nameof(landingPage));
                break;

            case ContentKind.Guide:
                if (guide is null)
                {
                    throw new ArgumentException(
                        "ContentKind.Guide requires a Guide specialization.",
                        nameof(guide));
                }

                if (article is not null || landingPage is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.Guide cannot carry Article or LandingPage specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, guide.ContentItemId, nameof(guide));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported ContentKind.");
        }
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Content code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Content name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }

    private static void EnsureSameContentItemId(ContentItemId expected, ContentItemId actual, string paramName)
    {
        if (expected != actual)
        {
            throw new ArgumentException("Specialization ContentItemId mismatch.", paramName);
        }
    }
}
