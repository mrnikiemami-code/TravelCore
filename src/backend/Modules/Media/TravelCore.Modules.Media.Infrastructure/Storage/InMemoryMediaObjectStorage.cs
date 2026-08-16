using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Storage;

/// <summary>
/// In-memory Media object storage for tests (architect-accepted T003 adapter).
/// </summary>
public sealed class InMemoryMediaObjectStorage : IMediaObjectStorage
{
    private readonly Dictionary<string, Entry> _objects = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public Task PutAsync(MediaObjectPutRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);
        var key = MediaAsset.NormalizeStorageKey(request.StorageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(request));

        using var ms = new MemoryStream();
        request.Content.CopyTo(ms);
        var bytes = ms.ToArray();
        var contentType = MediaAsset.NormalizeContentType(request.ContentType);

        lock (_gate)
        {
            if (_objects.ContainsKey(key))
            {
                throw new InvalidOperationException($"Storage object already exists for key '{key}'.");
            }

            _objects[key] = new Entry(bytes, contentType);
        }

        return Task.CompletedTask;
    }

    public Task<MediaObjectReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));

        lock (_gate)
        {
            if (!_objects.TryGetValue(key, out var entry))
            {
                return Task.FromResult<MediaObjectReadResult?>(null);
            }

            var stream = new MemoryStream(entry.Bytes, writable: false);
            return Task.FromResult<MediaObjectReadResult?>(
                new MediaObjectReadResult(stream, entry.ContentType, entry.Bytes.LongLength));
        }
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));

        lock (_gate)
        {
            return Task.FromResult(_objects.ContainsKey(key));
        }
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));

        lock (_gate)
        {
            _objects.Remove(key);
        }

        return Task.CompletedTask;
    }

    private sealed record Entry(byte[] Bytes, string ContentType);
}
