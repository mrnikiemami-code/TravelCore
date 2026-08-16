namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Stable app-proxy public URL helpers (P06-R4). StorageKey must never appear in public contracts.
/// </summary>
public static class MediaPresentationUrls
{
    public static string OriginalContent(Guid mediaAssetId)
        => $"/api/media/assets/{mediaAssetId:D}/content";

    public static string VariantContent(Guid mediaAssetId, string profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);
        return $"/api/media/assets/{mediaAssetId:D}/variants/{NormalizeProfileSegment(profile)}/content";
    }

    /// <summary>
    /// Public URL segments use lowercase profile names (large|medium|thumbnail).
    /// </summary>
    public static string NormalizeProfileSegment(string profile)
        => profile.Trim().ToLowerInvariant();
}

/// <summary>
/// Media-owned presentation mapping for P02 <c>MediaImagePresentation</c> consumption.
/// URLs are app-proxy paths — never object-storage / StorageKey / filesystem paths.
/// </summary>
public sealed record MediaAssetPresentationResponse(
    Guid MediaAssetId,
    string Status,
    string? OriginalContentUrl,
    int? Width,
    int? Height,
    double? FocalX,
    double? FocalY,
    string? ContentType,
    IReadOnlyList<MediaVariantPresentationItem> Variants,
    MediaAssetAltCaptionPresentation? AltCaption);

public sealed record MediaVariantPresentationItem(
    string Profile,
    string Status,
    string? ContentUrl,
    int? Width,
    int? Height,
    string? ContentType);

/// <summary>
/// Opens Ready original/variant bytes for anonymous public delivery (app proxy).
/// Does not expose StorageKey or provider topology.
/// </summary>
public interface IMediaContentDeliveryService
{
    Task<MediaContentDeliveryResult?> OpenOriginalAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<MediaContentDeliveryResult?> OpenVariantAsync(
        Guid mediaAssetId,
        string profile,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds Media presentation DTOs (app-proxy URLs + optional exact-locale alt/caption).
/// </summary>
public interface IMediaPresentationService
{
    Task<MediaAssetPresentationResponse?> GetPresentationAsync(
        Guid mediaAssetId,
        string? localeCode = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Streamable content opened via <see cref="IMediaObjectStorage"/> for HTTP delivery.
/// Caller/endpoint must dispose after writing the response.
/// </summary>
public sealed class MediaContentDeliveryResult : IAsyncDisposable, IDisposable
{
    public MediaContentDeliveryResult(
        Stream content,
        string contentType,
        long contentLength,
        Guid mediaAssetId,
        string representation)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        ContentLength = contentLength;
        MediaAssetId = mediaAssetId;
        Representation = representation ?? throw new ArgumentNullException(nameof(representation));
    }

    public Stream Content { get; }

    public string ContentType { get; }

    public long ContentLength { get; }

    public Guid MediaAssetId { get; }

    /// <summary>Logical representation label: <c>original</c> or lowercase profile.</summary>
    public string Representation { get; }

    public void Dispose() => Content.Dispose();

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
