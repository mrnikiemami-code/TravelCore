namespace TravelCore.Modules.Content.Contracts;

/// <summary>
/// Public DTO for a ContentItem. Never exposes EF entities.
/// Canonical identity is ContentItemId only (P08-R1) — no independent ArticleId/LandingPageId/GuideId.
/// Localized overlays (when requested) are exact-locale only (ADR 0008) — no silent cross-language invent.
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
    string? LocalizedExcerpt = null);

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
/// Upsert locale row for title/body/excerpt. Slug omitted until P08-R3 locks ownership.
/// </summary>
public sealed record UpsertContentItemTranslationRequest(
    string Title,
    string? Body = null,
    string? Excerpt = null);

public sealed record ContentItemTranslationResponse(
    Guid ContentItemId,
    string LocaleCode,
    string Title,
    string? Body,
    string? Excerpt,
    string UpdatedAt);

/// <summary>
/// Cross-module contract for ContentItem create/get/list + localization baseline (TC-P08-T003).
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

    Task<IReadOnlyList<ContentItemResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<ContentItemTranslationResponse> UpsertTranslationAsync(
        Guid contentItemId,
        string localeCode,
        UpsertContentItemTranslationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItemTranslationResponse>> ListTranslationsAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default);
}
