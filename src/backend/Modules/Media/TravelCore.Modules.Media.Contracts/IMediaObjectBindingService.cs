namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Binds a generated opaque storage key and puts binary bytes for an existing MediaAsset (T003).
/// Not a public upload endpoint — Access-backed upload remains T004.
/// </summary>
public interface IMediaObjectBindingService
{
    Task<MediaAssetResponse> PutAndBindAsync(
        Guid mediaAssetId,
        Stream content,
        string contentType,
        long? contentLength = null,
        CancellationToken cancellationToken = default);
}
