namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// ContentItem↔Category association owned by Content (same schema).
/// </summary>
public sealed class ContentItemCategory
{
    private ContentItemCategory()
    {
    }

    private ContentItemCategory(ContentItemId contentItemId, ContentCategoryId categoryId)
    {
        ContentItemId = contentItemId;
        CategoryId = categoryId;
    }

    public ContentItemId ContentItemId { get; private set; }

    public ContentCategoryId CategoryId { get; private set; }

    internal static ContentItemCategory Create(ContentItemId contentItemId, ContentCategoryId categoryId)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        if (categoryId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentCategoryId cannot be empty.", nameof(categoryId));
        }

        return new ContentItemCategory(contentItemId, categoryId);
    }
}
