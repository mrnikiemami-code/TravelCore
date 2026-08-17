namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience↔Guide relationship (P10-R7 · TC-P10-T006).
/// Experience owns the assignment; Party owns the person. Logical GuidePartyId only — no Tour-owned person.
/// </summary>
public sealed class ExperienceGuideAssignment
{
    public const int MaxEntriesPerExperience = 16;

    private ExperienceGuideAssignment()
    {
    }

    private ExperienceGuideAssignment(
        ExperienceGuideAssignmentId id,
        TourProductId tourProductId,
        Guid guidePartyId,
        ExperienceGuideRole role,
        string? note)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ExperienceGuideAssignmentId cannot be empty.", nameof(id));
        }

        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (guidePartyId == Guid.Empty)
        {
            throw new ArgumentException("GuidePartyId cannot be empty.", nameof(guidePartyId));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported ExperienceGuideRole.");
        }

        Id = id;
        TourProductId = tourProductId;
        GuidePartyId = guidePartyId;
        Role = role;
        Note = TourCatalogFactCode.NormalizeDetail(note);
    }

    public ExperienceGuideAssignmentId Id { get; private set; }

    public TourProductId TourProductId { get; private set; }

    /// <summary>Logical Party person identity (0..1 per assignment). Party remains SoR.</summary>
    public Guid GuidePartyId { get; private set; }

    public ExperienceGuideRole Role { get; private set; }

    /// <summary>Optional English baseline note (ADR 0008 — no dual-locale column pairs).</summary>
    public string? Note { get; private set; }

    internal static ExperienceGuideAssignment Create(
        ExperienceGuideAssignmentId id,
        TourProductId tourProductId,
        Guid guidePartyId,
        ExperienceGuideRole role,
        string? note)
        => new(id, tourProductId, guidePartyId, role, note);

    public static ExperienceGuideAssignment Reconstitute(
        ExperienceGuideAssignmentId id,
        TourProductId tourProductId,
        Guid guidePartyId,
        ExperienceGuideRole role,
        string? note)
        => new(id, tourProductId, guidePartyId, role, note);

    public void SetRole(ExperienceGuideRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported ExperienceGuideRole.");
        }

        Role = role;
    }

    public void SetNote(string? note) => Note = TourCatalogFactCode.NormalizeDetail(note);
}
