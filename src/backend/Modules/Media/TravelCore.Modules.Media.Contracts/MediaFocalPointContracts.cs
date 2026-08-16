namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Admin focal-point upsert. Both coordinates required in [0.0, 1.0], or both null to clear.
/// Coordinate system: normalized relative to image bounds; origin top-left; +X right; +Y down.
/// </summary>
public sealed record UpsertFocalPointRequest(double? FocalX, double? FocalY);

/// <summary>
/// Focal-point read DTO. Null coordinates mean unset (no default invent).
/// </summary>
public sealed record MediaFocalPointResponse(
    Guid MediaAssetId,
    double? FocalX,
    double? FocalY);

/// <summary>
/// Access-backed Admin set/get for MediaAsset focal metadata (TC-P06-T006).
/// Does not regenerate variants or apply crop pipelines.
/// </summary>
public interface IMediaFocalPointService
{
    Task<MediaFocalPointResponse> SetAsync(
        Guid mediaAssetId,
        UpsertFocalPointRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaFocalPointResponse?> GetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);
}
