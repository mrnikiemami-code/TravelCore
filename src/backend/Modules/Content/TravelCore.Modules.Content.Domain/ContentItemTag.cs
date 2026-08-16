namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// ContentItem↔Tag association owned by Content (same schema).
/// </summary>
public sealed class ContentItemTag
{
    private ContentItemTag()
    {
    }

    private ContentItemTag(ContentItemId contentItemId, ContentTagId tagId)
    {
        ContentItemId = contentItemId;
        TagId = tagId;
    }

    public ContentItemId ContentItemId { get; private set; }

    public ContentTagId TagId { get; private set; }

    internal static ContentItemTag Create(ContentItemId contentItemId, ContentTagId tagId)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        if (tagId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentTagId cannot be empty.", nameof(tagId));
        }

        return new ContentItemTag(contentItemId, tagId);
    }
}
