namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Ordered MediaAssetId row inside a Gallery block (logical Media id only — no Media FK).
/// </summary>
public sealed class ContentBlockGalleryItem
{
    private ContentBlockGalleryItem()
    {
    }

    private ContentBlockGalleryItem(ContentBlockId blockId, Guid mediaAssetId, int sortOrder)
    {
        BlockId = blockId;
        MediaAssetId = mediaAssetId;
        SortOrder = sortOrder;
    }

    public ContentBlockId BlockId { get; private set; }

    public Guid MediaAssetId { get; private set; }

    public int SortOrder { get; private set; }

    internal static ContentBlockGalleryItem Create(ContentBlockId blockId, Guid mediaAssetId, int sortOrder)
    {
        if (blockId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentBlockId cannot be empty.", nameof(blockId));
        }

        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        return new ContentBlockGalleryItem(blockId, mediaAssetId, sortOrder);
    }

    internal void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        SortOrder = sortOrder;
    }
}
