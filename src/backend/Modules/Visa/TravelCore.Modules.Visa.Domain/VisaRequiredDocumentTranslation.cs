using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Locale-specific name/notes for a required document. Locale rows only.
/// </summary>
public sealed class VisaRequiredDocumentTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int NameMaxLength = 200;
    public const int NotesMaxLength = 2000;

    private VisaRequiredDocumentTranslation()
    {
        LocaleCode = null!;
        Name = null!;
    }

    private VisaRequiredDocumentTranslation(
        VisaRequiredDocumentId requiredDocumentId,
        string localeCode,
        string name,
        string? notes,
        Instant updatedAt)
    {
        RequiredDocumentId = requiredDocumentId;
        LocaleCode = localeCode;
        Name = name;
        Notes = notes;
        UpdatedAt = updatedAt;
    }

    public VisaRequiredDocumentId RequiredDocumentId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Name { get; private set; }

    public string? Notes { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static VisaRequiredDocumentTranslation Create(
        VisaRequiredDocumentId requiredDocumentId,
        string localeCode,
        string name,
        string? notes,
        Instant now) =>
        new(
            requiredDocumentId,
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
            throw new ArgumentException($"Required document name max length is {NameMaxLength}.", nameof(name));
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
            throw new ArgumentException($"Required document notes max length is {NotesMaxLength}.", nameof(notes));
        }

        return trimmed;
    }
}
