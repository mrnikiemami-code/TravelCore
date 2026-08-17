using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Structured required-document fact for one VisaRequirementSet (TC-P17-T004 / P17-R4).
/// Not an uploaded applicant file and not a MediaAsset.
/// </summary>
public sealed class VisaRequiredDocument
{
    private readonly List<VisaRequiredDocumentTranslation> _translations = [];

    private VisaRequiredDocument()
    {
        Code = null!;
        RequirementLevel = null!;
    }

    private VisaRequiredDocument(
        VisaRequiredDocumentId id,
        VisaRequirementSetId visaRequirementSetId,
        string code,
        VisaRequirementLevel requirementLevel,
        int sortOrder,
        Instant createdAt)
    {
        Id = id;
        VisaRequirementSetId = visaRequirementSetId;
        Code = code;
        RequirementLevel = requirementLevel;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaRequiredDocumentId Id { get; private set; }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public string Code { get; private set; }

    public VisaRequirementLevel RequirementLevel { get; private set; }

    public int SortOrder { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyList<VisaRequiredDocumentTranslation> Translations => _translations;

    internal static VisaRequiredDocument Create(
        VisaRequiredDocumentId id,
        VisaRequirementSetId visaRequirementSetId,
        string code,
        string requirementLevel,
        int sortOrder,
        string localeCode,
        string name,
        Instant now,
        string? notes = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequiredDocumentId cannot be empty.", nameof(id));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");
        }

        var document = new VisaRequiredDocument(
            id,
            visaRequirementSetId,
            VisaRequirementCode.Normalize(code, nameof(code)),
            VisaRequirementLevel.Parse(requirementLevel),
            sortOrder,
            now);
        document.AddTranslation(localeCode, name, notes, now);
        return document;
    }

    public void AddTranslation(string localeCode, string name, string? notes, Instant now)
    {
        var translation = VisaRequiredDocumentTranslation.Create(Id, localeCode, name, notes, now);
        if (_translations.Any(t => t.LocaleCode == translation.LocaleCode))
        {
            throw new InvalidOperationException(
                $"Translation for locale '{translation.LocaleCode}' already exists.");
        }

        _translations.Add(translation);
        Touch(now);
    }

    private void Touch(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        UpdatedAt = now;
    }
}
