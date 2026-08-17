namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// UX-level Experience difficulty classification (P10-R6 · TC-P10-T005).
/// Structured for AI/search/facets — not a scoring engine.
/// </summary>
public enum ExperienceDifficulty : short
{
    Easy = 1,
    Moderate = 2,
    Challenging = 3,
    Strenuous = 4
}

/// <summary>Whether equipment is required or recommended (P10-R6).</summary>
public enum ExperienceEquipmentKind : short
{
    Required = 1,
    Recommended = 2
}

/// <summary>Guide assignment role on an Experience (P10-R7 · TC-P10-T006).</summary>
public enum ExperienceGuideRole : short
{
    Primary = 1,
    Assistant = 2
}
