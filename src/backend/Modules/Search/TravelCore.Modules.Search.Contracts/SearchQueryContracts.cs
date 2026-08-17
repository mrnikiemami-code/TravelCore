namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Future public Search query contract (TC-P15-T001). Shape only — no execution engine.
/// </summary>
public sealed record SearchQueryRequest(
    string? QueryText,
    string LocaleCode,
    IReadOnlyDictionary<string, string>? Criteria);

/// <summary>
/// Future public Search result contract. Compact discovery hits — not catalog entities.
/// </summary>
public sealed record SearchHit(
    string Kind,
    Guid SourceId,
    string Title,
    string? Slug);

public sealed record SearchQueryResponse(
    IReadOnlyList<SearchHit> Hits);
