namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Provider-neutral Media object-storage port (TC-P06-T003). No vendor SDK types.
/// </summary>
public interface IMediaObjectStorage
{
    Task PutAsync(
        MediaObjectPutRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaObjectReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Technical blob deletion capability only. Domain asset lifecycle (R8) remains separate.
    /// </summary>
    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}

public sealed record MediaObjectPutRequest(
    string StorageKey,
    Stream Content,
    string ContentType,
    long? ContentLength = null);

public sealed record MediaObjectReadResult(
    Stream Content,
    string ContentType,
    long ContentLength) : IAsyncDisposable, IDisposable
{
    public void Dispose() => Content.Dispose();

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
