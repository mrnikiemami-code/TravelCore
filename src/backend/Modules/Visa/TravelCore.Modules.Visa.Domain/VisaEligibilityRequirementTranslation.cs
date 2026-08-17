using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Locale-specific name/notes for an eligibility requirement. Locale rows only.
/// </summary>
public sealed class VisaEligibilityRequirementTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int NameMaxLength = 200;
    public const int NotesMaxLength = 2000;

    private VisaEligibilityRequirementTranslation()
    {
        LocaleCode = null!;
        Name = null!;
    }

    private VisaEligibilityRequirementTranslation(
        VisaEligibilityRequirementId eligibilityRequirementId,
        string localeCode,
        string name,
        string? notes,
        Instant updatedAt)
    {
        EligibilityRequirementId = eligibilityRequirementId;
        LocaleCode = localeCode;
        Name = name;
        Notes = notes;
        UpdatedAt = updatedAt;
    }

    public VisaEligibilityRequirementId EligibilityRequirementId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Name { get; private set; }

    public string? Notes { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static VisaEligibilityRequirementTranslation Create(
        VisaEligibilityRequirementId eligibilityRequirementId,
        string localeCode,
        string name,
        string? notes,
        Instant now) =>
        new(
            eligibilityRequirementId,
            VisaDefinitionTranslation.NormalizeLocaleCode(localeCode),
            NormalizeName(name),
            NormalizeNotes(notes),
            now);

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Eligibility requirement name max length is {NameMaxLength}.", nameof(name));
        }

        return trimmed;
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var trimmed = notes.Trim();
        if (trimmed.Length > NotesMaxLength)
        {
            throw new ArgumentException(
                $"Eligibility requirement notes max length is {NotesMaxLength}.",
                nameof(notes));
        }

        return trimmed;
    }
}
