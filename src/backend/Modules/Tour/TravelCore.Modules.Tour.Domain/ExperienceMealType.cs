namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Closed meal type for Experience day meal plan (P10-R5 · TC-P10-T004).
/// Product facts only — no Pricing / surcharge engine.
/// </summary>
public enum ExperienceMealType : short
{
    Breakfast = 1,
    Lunch = 2,
    Dinner = 3,
    Other = 4
}
