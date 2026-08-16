namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Content-owned logical Destination reference (P08-R5: 0..N; no cross-schema FK).
/// </summary>
public sealed class ContentItemDestination
{
    public const int MaxLinksPerContentItem = 32;

    private ContentItemDestination()
    {
    }

    private ContentItemDestination(ContentItemId contentItemId, Guid destinationId)
    {
        ContentItemId = contentItemId;
        DestinationId = destinationId;
    }

    public ContentItemId ContentItemId { get; private set; }

    /// <summary>Logical Destination identity only — never an EF navigation / cross-schema FK.</summary>
    public Guid DestinationId { get; private set; }

    internal static ContentItemDestination Create(ContentItemId contentItemId, Guid destinationId)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        return new ContentItemDestination(contentItemId, destinationId);
    }
}
