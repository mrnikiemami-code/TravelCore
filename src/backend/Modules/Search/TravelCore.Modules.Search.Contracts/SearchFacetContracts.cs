namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Facet definition shape (TC-P15-T004 / P15-R4). Meaning of attributes remains domain-owned.
/// </summary>
public sealed record FacetDefinition(
    string Key,
    string DisplayLabel,
    string AttributeKey);

/// <summary>
/// One facet bucket with optional display label and result count.
/// </summary>
public sealed record FacetValue(
    string Value,
    string? DisplayLabel,
    int Count);

/// <summary>
/// Aggregated facet result for a single key. Composition only — no engine in T004.
/// </summary>
public sealed record FacetResult(
    string Key,
    IReadOnlyList<FacetValue> Values);
