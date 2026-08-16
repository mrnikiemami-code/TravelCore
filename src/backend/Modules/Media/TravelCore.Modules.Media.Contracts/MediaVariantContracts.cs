namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Public DTO for a derived MediaVariant. Never exposes EF entities or public delivery URLs.
/// </summary>
public sealed record MediaVariantResponse(
    Guid Id,
    Guid MediaAssetId,
    string Profile,
    string Status,
    int? Width,
    int? Height,
    long? ByteSize,
    string? StorageKey,
    string? ContentType,
    string? FailureReason,
    string CreatedAt,
    string UpdatedAt);

/// <summary>
/// Synchronous Media-owned variant generation (P06-R3). Separate from upload; not Hangfire/queue.
/// </summary>
public interface IMediaVariantProcessingService
{
    Task<IReadOnlyList<MediaVariantResponse>> GenerateForAssetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaVariantResponse>> ListForAssetAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<MediaVariantResponse?> GetByIdAsync(
        Guid variantId,
        CancellationToken cancellationToken = default);
}
