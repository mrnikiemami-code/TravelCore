namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Structured eligibility fact for an Experience (P10-R6 · TC-P10-T005).
/// Product facts only — not a Booking eligibility rule engine.
/// </summary>
public sealed class ExperienceEligibilityRequirement
{
    public const int MaxEntriesPerExperience = 32;
    public const int ValueMaxLength = 128;

    private ExperienceEligibilityRequirement()
    {
        Code = null!;
    }

    private ExperienceEligibilityRequirement(
        TourProductId tourProductId,
        string code,
        string? value,
        string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Value = value;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }

    public string Code { get; private set; }

    /// <summary>Optional structured value (e.g. age "12", flag "true").</summary>
    public string? Value { get; private set; }

    /// <summary>Optional English baseline detail (ADR 0008 — no dual-locale column pairs).</summary>
    public string? Detail { get; private set; }

    internal static ExperienceEligibilityRequirement Create(
        TourProductId tourProductId,
        string code,
        string? value,
        string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        return new ExperienceEligibilityRequirement(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            NormalizeValue(value),
            TourCatalogFactCode.NormalizeDetail(detail));
    }

    public static ExperienceEligibilityRequirement Reconstitute(
        TourProductId tourProductId,
        string code,
        string? value,
        string? detail)
        => Create(tourProductId, code, value, detail);

    private static string? NormalizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > ValueMaxLength)
        {
            throw new ArgumentException($"Eligibility value max length is {ValueMaxLength}.", nameof(value));
        }

        return trimmed;
    }
}
