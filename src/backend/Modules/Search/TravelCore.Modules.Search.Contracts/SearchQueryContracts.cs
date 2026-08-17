namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Public Search query request (TC-P15-T007 / P15-R7). Structured filters only — no engine DSL.
/// </summary>
public sealed record SearchPublicQueryRequest(
    string LocaleCode,
    string? QueryText,
    IReadOnlyList<string>? EntityTypes,
    IReadOnlyDictionary<string, string>? StructuredFilters,
    string? Sort,
    int? PageSize,
    string? ContinuationToken,
    IReadOnlyList<string>? RequestedFacets);

/// <summary>
/// Public discovery hit. Compact presentation facts — not a domain entity.
/// </summary>
public sealed record SearchPublicHit(
    string Kind,
    Guid SourceId,
    string Title,
    string? Slug,
    string LocaleCode,
    RankingScoreMetadata? Ranking);

/// <summary>
/// Continuation-ready pagination metadata (engine-neutral).
/// </summary>
public sealed record SearchContinuation(
    string? NextContinuationToken,
    int? PageSize,
    int ReturnedCount);

/// <summary>
/// Public Search query response. Engine-neutral; may include requested facets.
/// </summary>
public sealed record SearchPublicQueryResponse(
    string LocaleCode,
    IReadOnlyList<SearchPublicHit> Hits,
    IReadOnlyList<FacetResult>? Facets,
    SearchContinuation Continuation,
    IReadOnlyDictionary<string, string>? ResultMetadata);

/// <summary>
/// Engine-neutral Search query port. Stub/empty execution is allowed until a physical engine is locked.
/// </summary>
public interface ISearchQueryService
{
    Task<SearchPublicQueryResponse> QueryAsync(
        SearchPublicQueryRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Legacy shape-only contracts from T001. Prefer <see cref="SearchPublicQueryRequest"/> for the public API.
/// </summary>
public sealed record SearchQueryRequest(
    string? QueryText,
    string LocaleCode,
    IReadOnlyDictionary<string, string>? Criteria);

public sealed record SearchHit(
    string Kind,
    Guid SourceId,
    string Title,
    string? Slug);

public sealed record SearchQueryResponse(
    IReadOnlyList<SearchHit> Hits);
