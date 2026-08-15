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
