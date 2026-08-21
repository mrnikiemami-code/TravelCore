using TravelCore.Modules.Media.Contracts;

namespace TravelCore.Modules.Destination.Contracts;

public sealed record DestinationResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? LocalizedName = null,
    string? LocalizedDescription = null,
    string? Locale = null);

public sealed record CreateDestinationRequest(
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode);

public sealed record UpsertDestinationTranslationRequest(
    string Name,
    string? Description,
    string? Slug = null);

public sealed record DestinationTranslationResponse(
    Guid DestinationId,
    string LocaleCode,
    string Name,
    string? Description,
    string? Slug);

public sealed record SetDestinationTranslationSlugRequest(string? Slug);

public sealed record DestinationSlugLookupResponse(
    Guid DestinationId,
    string LocaleCode,
    string Slug,
    string Kind,
    string Code,
    string EnglishName);

public sealed record SetDestinationGeoRequest(
    decimal? Latitude,
    decimal? Longitude);

public sealed record DestinationPathNode(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    int DepthFromRoot);

public sealed record DestinationPathResponse(
    Guid DestinationId,
    IReadOnlyList<DestinationPathNode> AncestorsRootFirst,
    DestinationPathNode Self,
    IReadOnlyList<DestinationPathNode> BreadcrumbRootFirst);

public sealed record DestinationDescendantsResponse(
    Guid DestinationId,
    int MaxDepth,
    IReadOnlyList<DestinationPathNode> Nodes);

public sealed record DestinationMediaLinkResponse(
    Guid DestinationId,
    Guid MediaAssetId,
    string Role,
    int SortOrder);

public sealed record SetDestinationCoverRequest(Guid MediaAssetId);

public sealed record DestinationMediaItemPresentation(
    Guid MediaAssetId,
    string Role,
    int SortOrder,
    MediaAssetPresentationResponse? Presentation);

/// <summary>
/// Destination Cover presentation compose (Option A — no Gallery).
/// </summary>
public sealed record DestinationMediaPresentationResponse(
    Guid DestinationId,
    DestinationMediaItemPresentation? Cover);

/// <summary>
/// Destination↔Media Cover ownership (TC-P32-T008 Option A). Media owns bytes; Destination owns Cover link.
/// </summary>
public interface IDestinationMediaService
{
    Task<DestinationMediaLinkResponse> SetCoverAsync(
        Guid destinationId,
        SetDestinationCoverRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveCoverAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationMediaLinkResponse>> ListMediaLinksAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<DestinationMediaPresentationResponse?> GetMediaPresentationAsync(
        Guid destinationId,
        string? locale = null,
        CancellationToken cancellationToken = default);
}

public interface IDestinationReadQuery
{
    Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DestinationResponse?> GetByIdAsync(Guid id, string? locale, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationTranslationResponse>> ListTranslationsAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<DestinationSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationPathNode>> ListAncestorsAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<DestinationPathResponse?> GetPathAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);

    Task<DestinationDescendantsResponse?> ListDescendantsAsync(
        Guid destinationId,
        int maxDepth,
        CancellationToken cancellationToken = default);
}
