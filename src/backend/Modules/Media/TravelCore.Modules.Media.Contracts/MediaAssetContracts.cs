namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Public DTO for a MediaAsset. Never exposes EF entities or consumer relationship fields.
/// FocalX/FocalY are normalized [0.0, 1.0] relative coordinates (origin top-left) when set.
/// </summary>
public sealed record MediaAssetResponse(
    Guid Id,
    string ContentType,
    long ByteSize,
    int? Width,
    int? Height,
    double? FocalX,
    double? FocalY,
    string? StorageKey,
    string Status,
    string CreatedAt,
    string UpdatedAt);

public sealed record CreateMediaAssetRequest(
    string ContentType,
    long ByteSize,
    int? Width = null,
    int? Height = null,
    string? StorageKey = null,
    string? Status = null);

/// <summary>
/// Cross-module contract for MediaAsset create/get/list (TC-P06-T002 baseline).
/// </summary>
public interface IMediaAssetService
{
    Task<MediaAssetResponse> CreateAsync(
        CreateMediaAssetRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaAssetResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaAssetResponse>> ListAsync(
        string? status = null,
        int take = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Access-backed Admin upload orchestration (TC-P06-T004). Not a public UGC endpoint.
/// </summary>
public interface IMediaUploadService
{
    Task<MediaAssetResponse> UploadAsync(
        Stream content,
        string contentType,
        string? fileName = null,
        long? contentLength = null,
        CancellationToken cancellationToken = default);
}
