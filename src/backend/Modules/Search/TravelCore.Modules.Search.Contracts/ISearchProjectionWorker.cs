namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Async projection worker port (TC-P15-T003). Skeleton only — no real queue/broker.
/// </summary>
public interface ISearchProjectionWorker
{
    Task<SearchProjectionProcessResult> ProcessAsync(
        SearchProjectionEvent projectionEvent,
        CancellationToken cancellationToken = default);
}

public sealed record SearchProjectionProcessResult(
    bool Applied,
    string Outcome);
