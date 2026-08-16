namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Closed Tour media roles (P09-R8) — Cover | Gallery only; no Hero/custom roles.
/// </summary>
public enum TourMediaRole : short
{
    Cover = 0,
    Gallery = 1
}

/// <summary>
/// Tour-owned media relationship row (gallery meaning SoR / P09-R8).
/// Persists logical MediaAssetId only — never StorageKey / URL / path.
/// </summary>
public sealed class TourProductMediaLink
{
    private TourProductMediaLink()
    {
    }

    private TourProductMediaLink(TourProductId tourProductId, Guid mediaAssetId, TourMediaRole role, int sortOrder)
    {
        TourProductId = tourProductId;
        MediaAssetId = mediaAssetId;
        Role = role;
        SortOrder = sortOrder;
    }

    public TourProductId TourProductId { get; private set; }

    /// <summary>Logical Media identity only (no cross-schema FK).</summary>
    public Guid MediaAssetId { get; private set; }

    public TourMediaRole Role { get; private set; }

    /// <summary>
    /// Gallery ordering (unique among Gallery for the TourProduct). Cover uses 0 with no reorder semantics.
    /// </summary>
    public int SortOrder { get; private set; }

    internal static TourProductMediaLink CreateCover(TourProductId tourProductId, Guid mediaAssetId)
    {
        EnsureIds(tourProductId, mediaAssetId);
        return new TourProductMediaLink(tourProductId, mediaAssetId, TourMediaRole.Cover, sortOrder: 0);
    }

    internal static TourProductMediaLink CreateGallery(TourProductId tourProductId, Guid mediaAssetId, int sortOrder)
    {
        EnsureIds(tourProductId, mediaAssetId);
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Gallery SortOrder must be >= 0.");
        }

        return new TourProductMediaLink(tourProductId, mediaAssetId, TourMediaRole.Gallery, sortOrder);
    }

    internal void SetGallerySortOrder(int sortOrder)
    {
        if (Role != TourMediaRole.Gallery)
        {
            throw new InvalidOperationException("Only Gallery links support SortOrder changes.");
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Gallery SortOrder must be >= 0.");
        }

        SortOrder = sortOrder;
    }

    private static void EnsureIds(TourProductId tourProductId, Guid mediaAssetId)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }
    }
}
