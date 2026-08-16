using NodaTime;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Derived MediaVariant aggregate (large/medium/thumbnail).
/// Original bytes remain on MediaAsset — this row never stores a duplicate original blob.
/// </summary>
public sealed class MediaVariant
{
    public const int ContentTypeMaxLength = 128;
    public const int StorageKeyMaxLength = 1024;
    public const int FailureReasonMaxLength = 1024;

    private MediaVariant()
    {
    }

    private MediaVariant(
        MediaVariantId id,
        MediaAssetId mediaAssetId,
        MediaVariantProfile profile,
        MediaVariantStatus status,
        int? width,
        int? height,
        long? byteSize,
        string? storageKey,
        string? contentType,
        string? failureReason,
        Instant createdAt)
    {
        Id = id;
        MediaAssetId = mediaAssetId;
        Profile = profile;
        Status = status;
        Width = width;
        Height = height;
        ByteSize = byteSize;
        StorageKey = storageKey;
        ContentType = contentType;
        FailureReason = failureReason;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public MediaVariantId Id { get; private set; }

    public MediaAssetId MediaAssetId { get; private set; }

    public MediaVariantProfile Profile { get; private set; }

    public MediaVariantStatus Status { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public long? ByteSize { get; private set; }

    public string? StorageKey { get; private set; }

    public string? ContentType { get; private set; }

    public string? FailureReason { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static MediaVariant CreateReady(
        MediaAssetId mediaAssetId,
        MediaVariantProfile profile,
        int width,
        int height,
        long byteSize,
        string storageKey,
        string contentType,
        Instant now,
        MediaVariantId? id = null)
    {
        EnsureProfile(profile);
        return new MediaVariant(
            id ?? MediaVariantId.New(),
            mediaAssetId,
            profile,
            MediaVariantStatus.Ready,
            MediaAsset.NormalizeDimension(width, nameof(width))
                ?? throw new ArgumentOutOfRangeException(nameof(width)),
            MediaAsset.NormalizeDimension(height, nameof(height))
                ?? throw new ArgumentOutOfRangeException(nameof(height)),
            MediaAsset.NormalizeByteSize(byteSize),
            MediaAsset.NormalizeStorageKey(storageKey)
                ?? throw new ArgumentException("StorageKey is required for Ready variants.", nameof(storageKey)),
            MediaAsset.NormalizeContentType(contentType),
            failureReason: null,
            now);
    }

    public static MediaVariant CreateNotRequired(
        MediaAssetId mediaAssetId,
        MediaVariantProfile profile,
        int sourceWidth,
        int sourceHeight,
        Instant now,
        MediaVariantId? id = null)
    {
        EnsureProfile(profile);
        return new MediaVariant(
            id ?? MediaVariantId.New(),
            mediaAssetId,
            profile,
            MediaVariantStatus.NotRequired,
            MediaAsset.NormalizeDimension(sourceWidth, nameof(sourceWidth))
                ?? throw new ArgumentOutOfRangeException(nameof(sourceWidth)),
            MediaAsset.NormalizeDimension(sourceHeight, nameof(sourceHeight))
                ?? throw new ArgumentOutOfRangeException(nameof(sourceHeight)),
            byteSize: null,
            storageKey: null,
            contentType: null,
            failureReason: null,
            now);
    }

    public static MediaVariant CreateFailed(
        MediaAssetId mediaAssetId,
        MediaVariantProfile profile,
        Instant now,
        string? failureReason = null,
        int? width = null,
        int? height = null,
        MediaVariantId? id = null)
    {
        EnsureProfile(profile);
        return new MediaVariant(
            id ?? MediaVariantId.New(),
            mediaAssetId,
            profile,
            MediaVariantStatus.Failed,
            MediaAsset.NormalizeDimension(width, nameof(width)),
            MediaAsset.NormalizeDimension(height, nameof(height)),
            byteSize: null,
            storageKey: null,
            contentType: null,
            NormalizeFailureReason(failureReason),
            now);
    }

    /// <summary>
    /// Idempotent regenerate: replace derived blob metadata carefully (caller owns storage Put/Delete).
    /// </summary>
    public void ReplaceAsReady(
        int width,
        int height,
        long byteSize,
        string storageKey,
        string contentType,
        Instant now)
    {
        Status = MediaVariantStatus.Ready;
        Width = MediaAsset.NormalizeDimension(width, nameof(width))
            ?? throw new ArgumentOutOfRangeException(nameof(width));
        Height = MediaAsset.NormalizeDimension(height, nameof(height))
            ?? throw new ArgumentOutOfRangeException(nameof(height));
        ByteSize = MediaAsset.NormalizeByteSize(byteSize);
        StorageKey = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required for Ready variants.", nameof(storageKey));
        ContentType = MediaAsset.NormalizeContentType(contentType);
        FailureReason = null;
        UpdatedAt = now;
    }

    public void ReplaceAsNotRequired(int sourceWidth, int sourceHeight, Instant now)
    {
        Status = MediaVariantStatus.NotRequired;
        Width = MediaAsset.NormalizeDimension(sourceWidth, nameof(sourceWidth))
            ?? throw new ArgumentOutOfRangeException(nameof(sourceWidth));
        Height = MediaAsset.NormalizeDimension(sourceHeight, nameof(sourceHeight))
            ?? throw new ArgumentOutOfRangeException(nameof(sourceHeight));
        ByteSize = null;
        StorageKey = null;
        ContentType = null;
        FailureReason = null;
        UpdatedAt = now;
    }

    public void MarkFailed(Instant now, string? failureReason = null)
    {
        Status = MediaVariantStatus.Failed;
        ByteSize = null;
        StorageKey = null;
        ContentType = null;
        FailureReason = NormalizeFailureReason(failureReason);
        UpdatedAt = now;
    }

    /// <summary>Returns previous storage key (if any) so callers can compensate/delete derived blobs.</summary>
    public string? TakePreviousStorageKeyForReplace() => StorageKey;

    private static void EnsureProfile(MediaVariantProfile profile)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unsupported MediaVariantProfile.");
        }
    }

    private static string? NormalizeFailureReason(string? failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            return null;
        }

        var trimmed = failureReason.Trim();
        return trimmed.Length <= FailureReasonMaxLength
            ? trimmed
            : trimmed[..FailureReasonMaxLength];
    }
}
