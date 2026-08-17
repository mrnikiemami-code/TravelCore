using TravelCore.Modules.Search.Contracts;

namespace TravelCore.Modules.Search.Infrastructure;

/// <summary>
/// Skeleton async projection worker (TC-P15-T003 / P15-R3). No broker/queue.
/// Idempotent acceptance only — document upsert wiring stays behind <see cref="ISearchIndex"/> later.
/// </summary>
public sealed class SearchProjectionWorker : ISearchProjectionWorker
{
    private readonly ISearchProjectionIdempotencyStore _idempotency;

    public SearchProjectionWorker(ISearchProjectionIdempotencyStore idempotency)
    {
        _idempotency = idempotency ?? throw new ArgumentNullException(nameof(idempotency));
    }

    public async Task<SearchProjectionProcessResult> ProcessAsync(
        SearchProjectionEvent projectionEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(projectionEvent);

        if (await _idempotency.HasProcessedAsync(projectionEvent.EventId, cancellationToken).ConfigureAwait(false))
        {
            return new SearchProjectionProcessResult(Applied: false, Outcome: "DuplicateSkipped");
        }

        await _idempotency.MarkProcessedAsync(
            projectionEvent.EventId,
            projectionEvent.Version,
            cancellationToken).ConfigureAwait(false);

        return new SearchProjectionProcessResult(Applied: true, Outcome: "AcceptedForProjection");
    }
}
