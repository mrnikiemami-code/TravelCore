namespace TravelCore.Modules.Content.Contracts;

/// <summary>
/// Public DTO for a ContentItem. Never exposes EF entities.
/// Canonical identity is ContentItemId only (P08-R1) — no independent ArticleId/LandingPageId/GuideId.
/// Localized overlays (when requested) are exact-locale only (ADR 0008) — no silent cross-language invent.
/// Category/Tag ids are Content-owned taxonomy links (T004). Author omitted (P08-R7 open).
/// </summary>
public sealed record ContentItemResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    ArticleDetailsResponse? Article,
    LandingPageDetailsResponse? LandingPage,
    GuideDetailsResponse? Guide,
    string CreatedAt,
    string UpdatedAt,
    string? LocalizedTitle = null,
    string? LocalizedBody = null,
    string? LocalizedExcerpt = null,
    IReadOnlyList<Guid>? CategoryIds = null,
    IReadOnlyList<Guid>? TagIds = null,
    IReadOnlyList<Guid>? DestinationIds = null);

/// <summary>Marker details for Article specialization (T002 — no type-specific fields yet).</summary>
public sealed record ArticleDetailsResponse();

/// <summary>Marker details for LandingPage specialization (T002 — no type-specific fields yet).</summary>
public sealed record LandingPageDetailsResponse();

/// <summary>Marker details for Guide specialization (T002 — no type-specific fields yet).</summary>
public sealed record GuideDetailsResponse();

/// <summary>
/// Create a ContentItem of one kind.
/// </summary>
public sealed record CreateContentItemRequest(
    string Kind,
    string Code,
    string EnglishName);

/// <summary>
/// Upsert locale row for title/body/excerpt. Slug is set via SetTranslationSlug (P08-R3).
/// </summary>
public sealed record UpsertContentItemTranslationRequest(
    string Title,
    string? Body = null,
    string? Excerpt = null);

public sealed record SetContentItemTranslationSlugRequest(string? Slug);

public sealed record ContentItemTranslationResponse(
    Guid ContentItemId,
    string LocaleCode,
    string Title,
    string? Body,
    string? Excerpt,
    string? Slug,
    string UpdatedAt);

/// <summary>
/// Public slug lookup hit (P08-R3). When publicOnly, requires a locale translation with title + slug.
/// Route existence ≠ indexing (P08-R4); SEO owns IndexPolicy.
/// </summary>
public sealed record ContentSlugLookupResponse(
    Guid ContentItemId,
    string LocaleCode,
    string Slug,
    string Kind,
    string Code,
    string EnglishName);

public sealed record ContentCategoryResponse(
    Guid Id,
    string Code,
    string EnglishName,
    string CreatedAt,
    string UpdatedAt);

public sealed record CreateContentCategoryRequest(
    string Code,
    string EnglishName);

public sealed record ContentTagResponse(
    Guid Id,
    string Code,
    string EnglishName,
    string CreatedAt,
    string UpdatedAt);

public sealed record CreateContentTagRequest(
    string Code,
    string EnglishName);

/// <summary>
/// Cross-module contract for ContentItem create/get/list + localization + taxonomy links (TC-P08-T004).
/// </summary>
public interface IContentItemService
{
    Task<ContentItemResponse> CreateAsync(
        CreateContentItemRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse?> GetByCodeAsync(
        string code,
        string? locale = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItemResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<ContentItemTranslationResponse> UpsertTranslationAsync(
        Guid contentItemId,
        string localeCode,
        UpsertContentItemTranslationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locale-specific slug lookup (P08-R3). When <paramref name="publicOnly"/> is true,
    /// only items with a non-empty title and slug for that locale are returned.
    /// </summary>
    Task<ContentSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        bool publicOnly = true,
        CancellationToken cancellationToken = default);

    Task<ContentItemTranslationResponse> SetTranslationSlugAsync(
        Guid contentItemId,
        string localeCode,
        SetContentItemTranslationSlugRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItemTranslationResponse>> ListTranslationsAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> AssignCategoryAsync(
        Guid contentItemId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> RemoveCategoryAsync(
        Guid contentItemId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> AssignTagAsync(
        Guid contentItemId,
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> RemoveTagAsync(
        Guid contentItemId,
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> AssignDestinationAsync(
        Guid contentItemId,
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse> RemoveDestinationAsync(
        Guid contentItemId,
        Guid destinationId,
        CancellationToken cancellationToken = default);
}

public sealed record ContentBlockGalleryItemResponse(Guid MediaAssetId, int SortOrder);

public sealed record ContentBlockFaqItemResponse(string Question, string Answer, int SortOrder);

public sealed record ContentBlockResponse(
    Guid Id,
    Guid ContentItemId,
    string Kind,
    int SortOrder,
    string? Text,
    short? HeadingLevel,
    Guid? MediaAssetId,
    string? Href,
    IReadOnlyList<ContentBlockGalleryItemResponse> GalleryItems,
    IReadOnlyList<ContentBlockFaqItemResponse> FaqItems);

public sealed record AddContentHeadingBlockRequest(string Text, short Level, int? SortOrder = null);

public sealed record AddContentParagraphBlockRequest(string Text, int? SortOrder = null);

public sealed record AddContentImageBlockRequest(Guid MediaAssetId, string? Caption = null, int? SortOrder = null);

public sealed record AddContentGalleryBlockRequest(IReadOnlyList<Guid> MediaAssetIds, int? SortOrder = null);

public sealed record ContentFaqItemRequest(string Question, string Answer);

public sealed record AddContentFaqBlockRequest(IReadOnlyList<ContentFaqItemRequest> Items, int? SortOrder = null);

public sealed record AddContentTableBlockRequest(string Text, int? SortOrder = null);

public sealed record AddContentVideoBlockRequest(Guid MediaAssetId, string? Caption = null, int? SortOrder = null);

public sealed record AddContentCtaBlockRequest(string Label, string Href, int? SortOrder = null);

public sealed record ReorderContentBlocksRequest(IReadOnlyList<Guid> OrderedBlockIds);

/// <summary>
/// Content-owned relational blocks API (TC-P08-T005 / P08-R2). No Tour/Place widgets (P08-R6 open).
/// </summary>
public interface IContentBlockService
{
    Task<ContentBlockResponse> AddHeadingAsync(
        Guid contentItemId,
        AddContentHeadingBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddParagraphAsync(
        Guid contentItemId,
        AddContentParagraphBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddImageAsync(
        Guid contentItemId,
        AddContentImageBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddGalleryAsync(
        Guid contentItemId,
        AddContentGalleryBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddFaqAsync(
        Guid contentItemId,
        AddContentFaqBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddTableAsync(
        Guid contentItemId,
        AddContentTableBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddVideoAsync(
        Guid contentItemId,
        AddContentVideoBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentBlockResponse> AddCtaAsync(
        Guid contentItemId,
        AddContentCtaBlockRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentBlockResponse>> ListAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentBlockResponse>> ReorderAsync(
        Guid contentItemId,
        ReorderContentBlocksRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid contentItemId,
        Guid blockId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Content-owned Category/Tag catalog (TC-P08-T004). Author deliberately omitted (P08-R7 open).
/// </summary>
public interface IContentTaxonomyService
{
    Task<ContentCategoryResponse> CreateCategoryAsync(
        CreateContentCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentCategoryResponse?> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentCategoryResponse>> ListCategoriesAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<ContentTagResponse> CreateTagAsync(
        CreateContentTagRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentTagResponse?> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentTagResponse>> ListTagsAsync(
        int take = 100,
        CancellationToken cancellationToken = default);
}
