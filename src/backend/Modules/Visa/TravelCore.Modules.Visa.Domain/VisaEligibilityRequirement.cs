using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Structured eligibility fact for one VisaRequirementSet (TC-P17-T004 / P17-R4).
/// Inspectable data only — not a rules engine, applicant entity, or executable predicate.
/// </summary>
public sealed class VisaEligibilityRequirement
{
    public const int ValueMaxLength = 64;
    public const int UnitMaxLength = 32;

    private readonly List<VisaEligibilityRequirementTranslation> _translations = [];

    private VisaEligibilityRequirement()
    {
        Code = null!;
        RequirementLevel = null!;
    }

    private VisaEligibilityRequirement(
        VisaEligibilityRequirementId id,
        VisaRequirementSetId visaRequirementSetId,
        string code,
        VisaRequirementLevel requirementLevel,
        string? kind,
        string? value,
        string? unit,
        int sortOrder,
        Instant createdAt)
    {
        Id = id;
        VisaRequirementSetId = visaRequirementSetId;
        Code = code;
        RequirementLevel = requirementLevel;
        Kind = kind;
        Value = value;
        Unit = unit;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaEligibilityRequirementId Id { get; private set; }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public string Code { get; private set; }

    public VisaRequirementLevel RequirementLevel { get; private set; }

    public string? Kind { get; private set; }

    public string? Value { get; private set; }

    public string? Unit { get; private set; }

    public int SortOrder { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyList<VisaEligibilityRequirementTranslation> Translations => _translations;

    internal static VisaEligibilityRequirement Create(
        VisaEligibilityRequirementId id,
        VisaRequirementSetId visaRequirementSetId,
        string code,
        string requirementLevel,
        int sortOrder,
        string localeCode,
        string name,
        Instant now,
        string? kind = null,
        string? value = null,
        string? unit = null,
        string? notes = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaEligibilityRequirementId cannot be empty.", nameof(id));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");
        }

        var requirement = new VisaEligibilityRequirement(
            id,
            visaRequirementSetId,
            VisaRequirementCode.Normalize(code, nameof(code)),
            VisaRequirementLevel.Parse(requirementLevel),
            VisaRequirementCode.NormalizeOptional(kind, nameof(kind)),
            NormalizeOptional(value, ValueMaxLength, nameof(value)),
            NormalizeOptional(unit, UnitMaxLength, nameof(unit)),
            sortOrder,
            now);
        requirement.AddTranslation(localeCode, name, notes, now);
        return requirement;
    }

    public void AddTranslation(string localeCode, string name, string? notes, Instant now)
    {
        var translation = VisaEligibilityRequirementTranslation.Create(Id, localeCode, name, notes, now);
        if (_translations.Any(t => t.LocaleCode == translation.LocaleCode))
        {
            throw new InvalidOperationException(
                $"Translation for locale '{translation.LocaleCode}' already exists.");
        }

        _translations.Add(translation);
        Touch(now);
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} max length is {maxLength}.", paramName);
        }

        return trimmed;
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
