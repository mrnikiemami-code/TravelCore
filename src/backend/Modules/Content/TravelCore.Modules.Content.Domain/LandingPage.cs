namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// LandingPage specialization (1:1 with <see cref="ContentItem"/> via <see cref="ContentItemId"/>).
/// Marker row only in T002 — type-specific editorial fields arrive in later tasks (no R2–R8 invention).
/// </summary>
public sealed class LandingPage
{
    private LandingPage()
    {
    }

    private LandingPage(ContentItemId contentItemId)
    {
        ContentItemId = contentItemId;
    }

    public ContentItemId ContentItemId { get; private set; }

    public static LandingPage Create(ContentItemId contentItemId)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        return new LandingPage(contentItemId);
    }
}
