namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// First-class relational Content Block (P08-R2). Ordering is relational SortOrder on ContentItem.
/// Widgets (Tour/Hotel/Attraction) are out of scope until P08-R6.
/// MediaAssetId refs are logical only — Media owns binaries; Content owns block meaning.
/// </summary>
public sealed class ContentBlock
{
    public const int TextMaxLength = 50_000;
    public const int HrefMaxLength = 500;
    public const int MaxGalleryItems = 64;
    public const int MaxFaqItems = 64;
    public const int MinHeadingLevel = 1;
    public const int MaxHeadingLevel = 6;

    private readonly List<ContentBlockGalleryItem> _galleryItems = [];
    private readonly List<ContentBlockFaqItem> _faqItems = [];

    private ContentBlock()
    {
    }

    private ContentBlock(
        ContentBlockId id,
        ContentItemId contentItemId,
        ContentBlockKind kind,
        int sortOrder,
        string? text,
        short? headingLevel,
        Guid? mediaAssetId,
        string? href)
    {
        Id = id;
        ContentItemId = contentItemId;
        Kind = kind;
        SortOrder = sortOrder;
        Text = text;
        HeadingLevel = headingLevel;
        MediaAssetId = mediaAssetId;
        Href = href;
    }

    public ContentBlockId Id { get; private set; }

    public ContentItemId ContentItemId { get; private set; }

    public ContentBlockKind Kind { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>Heading/paragraph/table/CTA primary text.</summary>
    public string? Text { get; private set; }

    public short? HeadingLevel { get; private set; }

    /// <summary>Logical MediaAssetId for Image/Video blocks (no cross-schema FK).</summary>
    public Guid? MediaAssetId { get; private set; }

    public string? Href { get; private set; }

    public IReadOnlyCollection<ContentBlockGalleryItem> GalleryItems => _galleryItems;

    public IReadOnlyCollection<ContentBlockFaqItem> FaqItems => _faqItems;

    internal static ContentBlock CreateHeading(
        ContentItemId contentItemId,
        int sortOrder,
        string text,
        short level,
        ContentBlockId? id = null)
    {
        if (level is < MinHeadingLevel or > MaxHeadingLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                $"Heading level must be {MinHeadingLevel}..{MaxHeadingLevel}.");
        }

        return new ContentBlock(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Heading,
            ValidateSortOrder(sortOrder),
            NormalizeRequiredText(text),
            level,
            mediaAssetId: null,
            href: null);
    }

    internal static ContentBlock CreateParagraph(
        ContentItemId contentItemId,
        int sortOrder,
        string text,
        ContentBlockId? id = null) =>
        new(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Paragraph,
            ValidateSortOrder(sortOrder),
            NormalizeRequiredText(text),
            headingLevel: null,
            mediaAssetId: null,
            href: null);

    internal static ContentBlock CreateImage(
        ContentItemId contentItemId,
        int sortOrder,
        Guid mediaAssetId,
        string? caption = null,
        ContentBlockId? id = null)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        return new ContentBlock(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Image,
            ValidateSortOrder(sortOrder),
            NormalizeOptionalText(caption),
            headingLevel: null,
            mediaAssetId,
            href: null);
    }

    internal static ContentBlock CreateGallery(
        ContentItemId contentItemId,
        int sortOrder,
        IReadOnlyList<Guid> mediaAssetIds,
        ContentBlockId? id = null)
    {
        ArgumentNullException.ThrowIfNull(mediaAssetIds);
        if (mediaAssetIds.Count == 0)
        {
            throw new ArgumentException("Gallery requires at least one MediaAssetId.", nameof(mediaAssetIds));
        }

        if (mediaAssetIds.Count > MaxGalleryItems)
        {
            throw new ArgumentException($"Gallery may have at most {MaxGalleryItems} items.", nameof(mediaAssetIds));
        }

        var blockId = id ?? ContentBlockId.New();
        var block = new ContentBlock(
            blockId,
            contentItemId,
            ContentBlockKind.Gallery,
            ValidateSortOrder(sortOrder),
            text: null,
            headingLevel: null,
            mediaAssetId: null,
            href: null);

        for (var i = 0; i < mediaAssetIds.Count; i++)
        {
            block._galleryItems.Add(ContentBlockGalleryItem.Create(blockId, mediaAssetIds[i], i));
        }

        return block;
    }

    internal static ContentBlock CreateFaq(
        ContentItemId contentItemId,
        int sortOrder,
        IReadOnlyList<(string Question, string Answer)> items,
        ContentBlockId? id = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("FAQ block requires at least one Q/A item.", nameof(items));
        }

        if (items.Count > MaxFaqItems)
        {
            throw new ArgumentException($"FAQ may have at most {MaxFaqItems} items.", nameof(items));
        }

        var blockId = id ?? ContentBlockId.New();
        var block = new ContentBlock(
            blockId,
            contentItemId,
            ContentBlockKind.Faq,
            ValidateSortOrder(sortOrder),
            text: null,
            headingLevel: null,
            mediaAssetId: null,
            href: null);

        for (var i = 0; i < items.Count; i++)
        {
            block._faqItems.Add(
                ContentBlockFaqItem.Create(blockId, items[i].Question, items[i].Answer, i));
        }

        return block;
    }

    internal static ContentBlock CreateTable(
        ContentItemId contentItemId,
        int sortOrder,
        string text,
        ContentBlockId? id = null) =>
        new(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Table,
            ValidateSortOrder(sortOrder),
            NormalizeRequiredText(text),
            headingLevel: null,
            mediaAssetId: null,
            href: null);

    internal static ContentBlock CreateVideo(
        ContentItemId contentItemId,
        int sortOrder,
        Guid mediaAssetId,
        string? caption = null,
        ContentBlockId? id = null)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        return new ContentBlock(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Video,
            ValidateSortOrder(sortOrder),
            NormalizeOptionalText(caption),
            headingLevel: null,
            mediaAssetId,
            href: null);
    }

    internal static ContentBlock CreateCta(
        ContentItemId contentItemId,
        int sortOrder,
        string label,
        string href,
        ContentBlockId? id = null) =>
        new(
            id ?? ContentBlockId.New(),
            contentItemId,
            ContentBlockKind.Cta,
            ValidateSortOrder(sortOrder),
            NormalizeRequiredText(label),
            headingLevel: null,
            mediaAssetId: null,
            href: NormalizeHref(href));

    internal void SetSortOrder(int sortOrder) => SortOrder = ValidateSortOrder(sortOrder);

    private static int ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        return sortOrder;
    }

    private static string NormalizeRequiredText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var trimmed = text.Trim();
        if (trimmed.Length > TextMaxLength)
        {
            throw new ArgumentException($"Block text max length is {TextMaxLength}.", nameof(text));
        }

        return trimmed;
    }

    private static string? NormalizeOptionalText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        if (trimmed.Length > TextMaxLength)
        {
            throw new ArgumentException($"Block text max length is {TextMaxLength}.", nameof(text));
        }

        return trimmed;
    }

    private static string NormalizeHref(string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        var trimmed = href.Trim();
        if (trimmed.Length > HrefMaxLength)
        {
            throw new ArgumentException($"Href max length is {HrefMaxLength}.", nameof(href));
        }

        return trimmed;
    }
}
