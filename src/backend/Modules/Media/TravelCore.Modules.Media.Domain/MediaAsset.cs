using NodaTime;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Media-owned technical asset aggregate.
/// Owns identity, MIME, size, dimensions, storage key metadata, and processing status.
/// Does not own consumer relationship meaning (hero/gallery order/role).
/// Does not store binary bytes in the metadata table.
/// </summary>
public sealed class MediaAsset
{
    public const int ContentTypeMaxLength = 128;
    public const int StorageKeyMaxLength = 1024;

    private MediaAsset()
    {
        ContentType = null!;
    }

    private MediaAsset(
        MediaAssetId id,
        string contentType,
        long byteSize,
        int? width,
        int? height,
        string? storageKey,
        MediaAssetStatus status,
        Instant createdAt)
    {
        Id = id;
        ContentType = contentType;
        ByteSize = byteSize;
        Width = width;
        Height = height;
        StorageKey = storageKey;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public MediaAssetId Id { get; private set; }

    /// <summary>MIME content type (e.g. image/jpeg). Technical fact only.</summary>
    public string ContentType { get; private set; }

    /// <summary>Declared byte length of the binary object. Not an in-row blob.</summary>
    public long ByteSize { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    /// <summary>
    /// Opaque storage object key/URI placeholder. Provider wiring is TC-P06-T003+.
    /// </summary>
    public string? StorageKey { get; private set; }

    public MediaAssetStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Binds an opaque storage key after binary put (TC-P06-T003). Does not imply public delivery.
    /// </summary>
    public void BindStorageKey(string storageKey, Instant now, MediaAssetStatus? status = null)
    {
        if (!string.IsNullOrWhiteSpace(StorageKey))
        {
            throw new InvalidOperationException("MediaAsset already has a StorageKey binding.");
        }

        StorageKey = NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));
        if (status is not null)
        {
            if (!Enum.IsDefined(status.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported MediaAssetStatus.");
            }

            Status = status.Value;
        }

        UpdatedAt = now;
    }

    /// <summary>
    /// Marks the asset as Failed after upload/validation/storage error (TC-P06-T004).
    /// Does not decide domain delete lifecycle (P06-R8 remains open).
    /// </summary>
    public void MarkFailed(Instant now)
    {
        Status = MediaAssetStatus.Failed;
        UpdatedAt = now;
    }

    /// <summary>
    /// Records decoded source dimensions during variant processing. Does not change Status.
    /// </summary>
    public void SetDimensions(int width, int height, Instant now)
    {
        Width = NormalizeDimension(width, nameof(width));
        Height = NormalizeDimension(height, nameof(height));
        UpdatedAt = now;
    }

    public static MediaAsset Create(
        string contentType,
        long byteSize,
        Instant now,
        int? width = null,
        int? height = null,
        string? storageKey = null,
        MediaAssetStatus status = MediaAssetStatus.PendingStorage,
        MediaAssetId? id = null)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported MediaAssetStatus.");
        }

        return new MediaAsset(
            id ?? MediaAssetId.New(),
            NormalizeContentType(contentType),
            NormalizeByteSize(byteSize),
            NormalizeDimension(width, nameof(width)),
            NormalizeDimension(height, nameof(height)),
            NormalizeStorageKey(storageKey),
            status,
            now);
    }

    public static string NormalizeContentType(string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        var normalized = contentType.Trim().ToLowerInvariant();
        if (normalized.Length > ContentTypeMaxLength)
        {
            throw new ArgumentException(
                $"ContentType exceeds max length {ContentTypeMaxLength}.",
                nameof(contentType));
        }

        if (normalized.Contains('/', StringComparison.Ordinal) is false)
        {
            throw new ArgumentException(
                "ContentType must be a MIME type (type/subtype).",
                nameof(contentType));
        }

        return normalized;
    }

    public static long NormalizeByteSize(long byteSize)
    {
        if (byteSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteSize), byteSize, "ByteSize cannot be negative.");
        }

        return byteSize;
    }

    public static int? NormalizeDimension(int? value, string paramName)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Dimension must be positive when provided.");
        }

        return value;
    }

    public static string? NormalizeStorageKey(string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return null;
        }

        var normalized = storageKey.Trim();
        if (normalized.Length > StorageKeyMaxLength)
        {
            throw new ArgumentException(
                $"StorageKey exceeds max length {StorageKeyMaxLength}.",
                nameof(storageKey));
        }

        if (normalized.Contains("..", StringComparison.Ordinal)
            || normalized.Contains("\\", StringComparison.Ordinal)
            || normalized.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "StorageKey must be a relative opaque object key without path traversal.",
                nameof(storageKey));
        }

        return normalized;
    }
}
