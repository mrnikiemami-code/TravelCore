namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Replaceable deterministic ranking port (TC-P15-T005 / P15-R5). No ML/recommendation engine.
/// </summary>
public interface ISearchRanker
{
    IReadOnlyList<RankingResult> Rank(
        RankingContext context,
        IReadOnlyList<RankedCandidate> candidates);
}

/// <summary>
/// Candidate with engine-neutral signals for ranking composition.
/// </summary>
public sealed record RankedCandidate(
    Guid SourceId,
    IReadOnlyList<RankingSignal> Signals);
