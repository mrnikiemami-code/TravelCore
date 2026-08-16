namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Per-locale publication lifecycle for MediaAsset alt/caption (ADR 0008).
/// Row existence ≠ Published. Public presentation may only surface Published rows.
/// </summary>
public enum MediaTranslationPublicationStatus : short
{
    Draft = 0,
    Ready = 1,
    Published = 2,
    Archived = 3
}
