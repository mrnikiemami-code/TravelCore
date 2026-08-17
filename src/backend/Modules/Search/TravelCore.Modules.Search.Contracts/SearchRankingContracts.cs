namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Engine-neutral ranking signal (TC-P15-T005 / P15-R5). Not a business-policy invent.
/// </summary>
public sealed record RankingSignal(
    string Kind,
    decimal Value,
    string? Source);

/// <summary>
/// Context for composing ranking. No personalization / recommendation profile.
/// </summary>
public sealed record RankingContext(
    string LocaleCode,
    string? QueryText,
    IReadOnlyDictionary<string, string>? Criteria);

/// <summary>
/// Ranking outcome metadata for a single hit. Explainability keys allowed; no business-sensitive invent.
/// </summary>
public sealed record RankingScoreMetadata(
    decimal Score,
    string TieBreakKey,
    IReadOnlyDictionary<string, string>? Diagnostics);

/// <summary>
/// Ordered ranking result for one discovery hit.
/// </summary>
public sealed record RankingResult(
    Guid SourceId,
    int Ordinal,
    RankingScoreMetadata ScoreMetadata);
