namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Destination-owned media relationship row (Cover meaning SoR).
/// Persists logical MediaAssetId only — never StorageKey / URL / path.
/// </summary>
public sealed class DestinationMediaLink
{
    private DestinationMediaLink()
    {
    }

    private DestinationMediaLink(
        DestinationId destinationId,
        Guid mediaAssetId,
        DestinationMediaRole role,
        int sortOrder)
    {
        DestinationId = destinationId;
        MediaAssetId = mediaAssetId;
        Role = role;
        SortOrder = sortOrder;
    }

    public DestinationId DestinationId { get; private set; }

    /// <summary>Logical Media identity only (no cross-schema FK).</summary>
    public Guid MediaAssetId { get; private set; }

    public DestinationMediaRole Role { get; private set; }

    /// <summary>
    /// Cover uses SortOrder 0 with no reorder semantics (Gallery deferred).
    /// </summary>
    public int SortOrder { get; private set; }

    internal static DestinationMediaLink CreateCover(DestinationId destinationId, Guid mediaAssetId)
    {
        EnsureIds(destinationId, mediaAssetId);
        return new DestinationMediaLink(destinationId, mediaAssetId, DestinationMediaRole.Cover, sortOrder: 0);
    }

    private static void EnsureIds(DestinationId destinationId, Guid mediaAssetId)
    {
        if (destinationId.Value == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }
    }
}
