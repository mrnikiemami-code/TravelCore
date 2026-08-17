using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Context-dependent requirement facts for one VisaDefinition (TC-P17-T002 / P17-R2).
/// Owns exactly one VisaApplicability (TC-P17-T003 / P17-R3), document/eligibility children (P17-R4),
/// and distinct processing/validity/stay/entry facts (TC-P17-T005 / P17-R5). Fees remain later R#.
/// </summary>
public sealed class VisaRequirementSet
{
    private readonly List<VisaRequiredDocument> _requiredDocuments = [];
    private readonly List<VisaEligibilityRequirement> _eligibilityRequirements = [];
    private VisaApplicability _applicability = null!;
    private VisaProcessingTime? _processingTime;
    private VisaValidity? _validity;
    private VisaAllowedStay? _allowedStay;
    private VisaEntryPolicy? _entryPolicy;

    private VisaRequirementSet()
    {
    }

    private VisaRequirementSet(VisaRequirementSetId id, VisaDefinitionId visaDefinitionId, Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(id));
        }

        if (visaDefinitionId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaDefinitionId cannot be empty.", nameof(visaDefinitionId));
        }

        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        Id = id;
        VisaDefinitionId = visaDefinitionId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaRequirementSetId Id { get; private set; }

    /// <summary>Owning VisaDefinition. Same-schema relationship only.</summary>
    public VisaDefinitionId VisaDefinitionId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>Optional fact-window start. Readiness only — not a versioning engine.</summary>
    public Instant? EffectiveFrom { get; private set; }

    /// <summary>Optional fact-window end. Readiness only — not a versioning engine.</summary>
    public Instant? EffectiveTo { get; private set; }

    /// <summary>Exactly one structured applicability context. Not a rules engine.</summary>
    public VisaApplicability Applicability => _applicability;

    /// <summary>Issuance/review processing time. Not validity, stay, or entry.</summary>
    public VisaProcessingTime? ProcessingTime => _processingTime;

    /// <summary>How long an issued visa remains valid. Not processing time or stay.</summary>
    public VisaValidity? Validity => _validity;

    /// <summary>Maximum allowed presence. Not visa validity or processing time.</summary>
    public VisaAllowedStay? AllowedStay => _allowedStay;

    /// <summary>Entry count/policy. Not inferred from any time quantity.</summary>
    public VisaEntryPolicy? EntryPolicy => _entryPolicy;

    public IReadOnlyList<VisaRequiredDocument> RequiredDocuments => _requiredDocuments;

    public IReadOnlyList<VisaEligibilityRequirement> EligibilityRequirements => _eligibilityRequirements;

    internal static VisaRequirementSet Create(
        VisaRequirementSetId id,
        VisaDefinitionId visaDefinitionId,
        Guid destinationGeographicId,
        Instant now,
        string? applicantNationalityCode = null,
        string? residenceCountryCode = null,
        string? applicantCategory = null)
    {
        var set = new VisaRequirementSet(id, visaDefinitionId, now);
        set._applicability = VisaApplicability.Create(
            id,
            destinationGeographicId,
            applicantNationalityCode,
            residenceCountryCode,
            applicantCategory);
        return set;
    }

    public VisaRequiredDocument AddRequiredDocument(
        string code,
        string requirementLevel,
        string localeCode,
        string name,
        Instant now,
        string? notes = null,
        int sortOrder = 0)
    {
        var normalized = VisaRequirementCode.Normalize(code, nameof(code));
        if (_requiredDocuments.Any(d => d.Code == normalized))
        {
            throw new InvalidOperationException($"Required document '{normalized}' already exists.");
        }

        var document = VisaRequiredDocument.Create(
            VisaRequiredDocumentId.New(),
            Id,
            normalized,
            requirementLevel,
            sortOrder,
            localeCode,
            name,
            now,
            notes);
        _requiredDocuments.Add(document);
        Touch(now);
        return document;
    }

    public VisaEligibilityRequirement AddEligibilityRequirement(
        string code,
        string requirementLevel,
        string localeCode,
        string name,
        Instant now,
        string? kind = null,
        string? value = null,
        string? unit = null,
        string? notes = null,
        int sortOrder = 0)
    {
        var normalized = VisaRequirementCode.Normalize(code, nameof(code));
        if (_eligibilityRequirements.Any(d => d.Code == normalized))
        {
            throw new InvalidOperationException($"Eligibility requirement '{normalized}' already exists.");
        }

        var requirement = VisaEligibilityRequirement.Create(
            VisaEligibilityRequirementId.New(),
            Id,
            normalized,
            requirementLevel,
            sortOrder,
            localeCode,
            name,
            now,
            kind,
            value,
            unit,
            notes);
        _eligibilityRequirements.Add(requirement);
        Touch(now);
        return requirement;
    }

    public VisaProcessingTime SetProcessingTime(int minValue, string unit, Instant now, int? maxValue = null)
    {
        _processingTime = VisaProcessingTime.Create(Id, minValue, maxValue, unit);
        Touch(now);
        return _processingTime;
    }

    public VisaValidity SetValidity(int value, string unit, Instant now)
    {
        _validity = VisaValidity.Create(Id, value, unit);
        Touch(now);
        return _validity;
    }

    public VisaAllowedStay SetAllowedStay(int value, string unit, Instant now)
    {
        _allowedStay = VisaAllowedStay.Create(Id, value, unit);
        Touch(now);
        return _allowedStay;
    }

    public VisaEntryPolicy SetEntryPolicy(string kind, Instant now)
    {
        _entryPolicy = VisaEntryPolicy.Create(Id, kind);
        Touch(now);
        return _entryPolicy;
    }

    public void SetEffectivePeriod(Instant? effectiveFrom, Instant? effectiveTo, Instant now)
    {
        if (effectiveFrom is Instant from && from == default)
        {
            throw new ArgumentException("EffectiveFrom cannot be default Instant.", nameof(effectiveFrom));
        }

        if (effectiveTo is Instant to && to == default)
        {
            throw new ArgumentException("EffectiveTo cannot be default Instant.", nameof(effectiveTo));
        }

        if (effectiveFrom is Instant start && effectiveTo is Instant end && end < start)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveTo), "EffectiveTo cannot be before EffectiveFrom.");
        }

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
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
