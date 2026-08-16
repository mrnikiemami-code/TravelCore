namespace TravelCore.Modules.Content.Contracts;

/// <summary>
/// Public DTO for a ContentItem. Never exposes EF entities.
/// Canonical identity is ContentItemId only (P08-R1) — no independent ArticleId/LandingPageId/GuideId.
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
    string UpdatedAt);

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
/// Cross-module contract for ContentItem create/get/list (TC-P08-T002 baseline).
/// </summary>
public interface IContentItemService
{
    Task<ContentItemResponse> CreateAsync(
        CreateContentItemRequest request,
        CancellationToken cancellationToken = default);

    Task<ContentItemResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItemResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);
}
