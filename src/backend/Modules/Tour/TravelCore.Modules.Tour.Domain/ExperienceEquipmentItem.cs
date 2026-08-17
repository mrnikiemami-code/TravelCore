namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Structured equipment fact for an Experience (P10-R6 · TC-P10-T005).
/// AI/UX-ready codes — not inventory ownership.
/// </summary>
public sealed class ExperienceEquipmentItem
{
    public const int MaxEntriesPerExperience = 32;

    private ExperienceEquipmentItem()
    {
        Code = null!;
    }

    private ExperienceEquipmentItem(
        TourProductId tourProductId,
        string code,
        ExperienceEquipmentKind kind,
        string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Kind = kind;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }

    public string Code { get; private set; }

    public ExperienceEquipmentKind Kind { get; private set; }

    /// <summary>Optional English baseline detail (ADR 0008).</summary>
    public string? Detail { get; private set; }

    internal static ExperienceEquipmentItem Create(
        TourProductId tourProductId,
        string code,
        ExperienceEquipmentKind kind,
        string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported ExperienceEquipmentKind.");
        }

        return new ExperienceEquipmentItem(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            kind,
            TourCatalogFactCode.NormalizeDetail(detail));
    }

    public static ExperienceEquipmentItem Reconstitute(
        TourProductId tourProductId,
        string code,
        ExperienceEquipmentKind kind,
        string? detail)
        => Create(tourProductId, code, kind, detail);
}
