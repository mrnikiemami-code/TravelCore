namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Idempotency store for Search projection consumption (TC-P15-T003).
/// Skeleton only — no broker; consumers must tolerate retries/duplicates.
/// </summary>
public interface ISearchProjectionIdempotencyStore
{
    Task<bool> HasProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        Guid eventId,
        long sourceVersion,
        CancellationToken cancellationToken = default);
}
