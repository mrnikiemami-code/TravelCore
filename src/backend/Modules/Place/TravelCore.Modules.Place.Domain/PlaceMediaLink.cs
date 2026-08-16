namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place-owned media relationship row (gallery meaning SoR).
/// Persists logical MediaAssetId only — never StorageKey / URL / path.
/// </summary>
public sealed class PlaceMediaLink
{
    private PlaceMediaLink()
    {
    }

    private PlaceMediaLink(PlaceId placeId, Guid mediaAssetId, PlaceMediaRole role, int sortOrder)
    {
        PlaceId = placeId;
        MediaAssetId = mediaAssetId;
        Role = role;
        SortOrder = sortOrder;
    }

    public PlaceId PlaceId { get; private set; }

    /// <summary>Logical Media identity only (no cross-schema FK).</summary>
    public Guid MediaAssetId { get; private set; }

    public PlaceMediaRole Role { get; private set; }

    /// <summary>
    /// Gallery ordering (unique among Gallery for the Place). Cover uses 0 with no reorder semantics.
    /// </summary>
    public int SortOrder { get; private set; }

    internal static PlaceMediaLink CreateCover(PlaceId placeId, Guid mediaAssetId)
    {
        EnsureIds(placeId, mediaAssetId);
        return new PlaceMediaLink(placeId, mediaAssetId, PlaceMediaRole.Cover, sortOrder: 0);
    }

    internal static PlaceMediaLink CreateGallery(PlaceId placeId, Guid mediaAssetId, int sortOrder)
    {
        EnsureIds(placeId, mediaAssetId);
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Gallery SortOrder must be >= 0.");
        }

        return new PlaceMediaLink(placeId, mediaAssetId, PlaceMediaRole.Gallery, sortOrder);
    }

    internal void SetGallerySortOrder(int sortOrder)
    {
        if (Role != PlaceMediaRole.Gallery)
        {
            throw new InvalidOperationException("Only Gallery links support SortOrder changes.");
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Gallery SortOrder must be >= 0.");
        }

        SortOrder = sortOrder;
    }

    private static void EnsureIds(PlaceId placeId, Guid mediaAssetId)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }
    }
}
