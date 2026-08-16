namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Closed Content Block kinds for P08 editorial baseline (P08-R2).
/// Tour/Hotel/Attraction widgets deferred (P08-R6 UNRESOLVED) — not included.
/// </summary>
public enum ContentBlockKind : short
{
    Heading = 1,
    Paragraph = 2,
    Image = 3,
    Gallery = 4,
    Faq = 5,
    Table = 6,
    Video = 7,
    Cta = 8
}
