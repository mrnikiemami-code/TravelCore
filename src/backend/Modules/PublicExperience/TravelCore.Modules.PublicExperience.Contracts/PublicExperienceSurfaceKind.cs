namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// Public Experience Layer surfaces (TC-P14-T001 / P14-R1).
/// Presentation + SEO composition only — not Search engine and not Tour catalog ownership.
/// </summary>
public enum PublicExperienceSurfaceKind : short
{
    Detail = 1,
    Listing = 2,
    Landing = 3
}
