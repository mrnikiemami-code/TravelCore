using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Canonical Visa-owned visa-type definition (TC-P17-T002 / P17-R2).
/// Represents what the visa type is (Tourist/Business/Transit conceptually).
/// Does not own destination/applicant applicability, documents, fees, or processing.
/// </summary>
public sealed class VisaDefinition
{
    public const int CodeMaxLength = 32;

    private readonly List<VisaDefinitionTranslation> _translations = [];
    private readonly List<VisaRequirementSet> _requirementSets = [];

    private VisaDefinition()
    {
        Code = null!;
    }

    private VisaDefinition(VisaDefinitionId id, string code, Instant createdAt)
    {
        Id = id;
        Code = code;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaDefinitionId Id { get; private set; }

    /// <summary>Stable visa-type code. Not a hardcoded country catalog.</summary>
    public string Code { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyList<VisaDefinitionTranslation> Translations => _translations;

    public IReadOnlyList<VisaRequirementSet> RequirementSets => _requirementSets;

    public static VisaDefinition Create(
        string code,
        string localeCode,
        string name,
        Instant now,
        string? summary = null)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        var definition = new VisaDefinition(VisaDefinitionId.New(), NormalizeCode(code), now);
        definition.AddTranslation(localeCode, name, summary, now);
        return definition;
    }

    public void AddTranslation(string localeCode, string name, string? summary, Instant now)
    {
        var translation = VisaDefinitionTranslation.Create(Id, localeCode, name, summary, now);
        if (_translations.Any(t => t.LocaleCode == translation.LocaleCode))
        {
            throw new InvalidOperationException(
                $"Translation for locale '{translation.LocaleCode}' already exists.");
        }

        _translations.Add(translation);
        Touch(now);
    }

    public VisaRequirementSet AddRequirementSet(
        Guid destinationGeographicId,
        Instant now,
        string? applicantNationalityCode = null,
        string? residenceCountryCode = null,
        string? applicantCategory = null)
    {
        var set = VisaRequirementSet.Create(
            VisaRequirementSetId.New(),
            Id,
            destinationGeographicId,
            now,
            applicantNationalityCode,
            residenceCountryCode,
            applicantCategory);
        _requirementSets.Add(set);
        Touch(now);
        return set;
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim().ToUpperInvariant();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Visa definition code max length is {CodeMaxLength}.", nameof(code));
        }

        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-')))
        {
            throw new ArgumentException(
                "Visa definition code may contain only A-Z, 0-9, hyphen, and underscore.",
                nameof(code));
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
